using Metaphysics.Core;

namespace Metaphysics.UnitTests;

[TestClass]
public class SimulationTests
{
    [TestMethod]
    public void Parent_IsNull_WhenNotProvided()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        Assert.IsNull(simulation.Parent);
    }

    [TestMethod]
    public void Parent_IsSet_WhenProvided()
    {
        using var parent = new Simulation(SimulationClass.Base);
        using var child = new Simulation(SimulationClass.Base, parent);
        Assert.AreEqual(parent, child.Parent);
    }

    [TestMethod]
    public void AvailableResources_IsEmpty_OnCreation()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        Assert.IsEmpty(simulation.AvailableResources);
    }

    [TestMethod]
    public void AddAvailableResource_AddsResourceToList()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var resource = new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false);

        simulation.AddAvailableResource(resource);

        Assert.HasCount(1, simulation.AvailableResources);
        Assert.AreEqual(resource, simulation.AvailableResources[0]);
    }

    [TestMethod]
    public void UsedUpResources_IsEmpty_OnCreation()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void UseUpResource_MovesResourceToWasteList()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false));
        var waste = new SimulationResource(ResourceType.MetaphysicalEnergy, 40m, false);

        simulation.UseUpResource(waste);

        Assert.HasCount(1, simulation.UsedUpResources);
        Assert.AreEqual(waste, simulation.UsedUpResources[0]);
        Assert.AreEqual(60m, simulation.AvailableResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void UseUpResource_ConsumesAcrossMultipleEntries()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 30m, false));
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false));

        simulation.UseUpResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 70m, false));

        Assert.AreEqual(10m, simulation.AvailableResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void UseUpResource_Throws_WhenInsufficientResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.UseUpResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false)));
    }

    [TestMethod]
    public void AddAvailableResource_MergesIntoSingleEntryPerResourceType()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var resource1 = new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false);
        var resource2 = new SimulationResource(ResourceType.MetaphysicalEnergy, 75m, false);

        simulation.AddAvailableResource(resource1);
        simulation.AddAvailableResource(resource2);

        Assert.HasCount(1, simulation.AvailableResources);
        Assert.AreEqual(125m, simulation.AvailableResources[0].Quantity);
    }

    [TestMethod]
    public void EntitiesByIndividualId_IsEmpty_OnCreation()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        Assert.IsEmpty(simulation.EntitiesByIndividualId);
    }

    [TestMethod]
    public void AddOrChangeEntities_PopulatesEntitiesByIndividualId_WhenIndividualIdIsSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");
        var id = Guid.NewGuid();
        entity.IndividualId = id;

        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [entity], simulation);

        Assert.HasCount(1, simulation.EntitiesByIndividualId);
        Assert.AreEqual(entity, simulation.EntitiesByIndividualId[id]);
    }

    [TestMethod]
    public void AddOrChangeEntities_DoesNotPopulateEntitiesByIndividualId_WhenIndividualIdIsNotSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");

        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [entity], simulation);

        Assert.IsEmpty(simulation.EntitiesByIndividualId);
    }

    [TestMethod]
    public void AddOrChangeEntities_ReplacesBeforeEntityWithAfterEntity_WhenBeforeEntityIsNonNull()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var before = new SimulationEntity("Before");
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [before], simulation);

        var after = new SimulationEntity("After");
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { before, after } }, [], simulation);

        Assert.HasCount(1, simulation.Entities);
        Assert.AreEqual(after, simulation.Entities[0]);
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenChangingFromDeceasedToAlive()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var before = new SimulationEntity("Before") { Status = SimulationEntityStatus.Deceased };
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [before], simulation);

        var after = new SimulationEntity("After");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { before, after } }, [], simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_OnDeath_RemovesEntityAndReallocatesResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        var alive = new SimulationEntity("Organism");
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, true));
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [alive], simulation);

        var deceased = new SimulationEntity(alive) { Status = SimulationEntityStatus.Deceased };
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { alive, deceased } }, [], simulation);

        Assert.IsEmpty(simulation.Entities);
        Assert.HasCount(1, simulation.AvailableResources);
        Assert.AreEqual(5m, simulation.AvailableResources[0].Quantity);
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_ReflectEntityValueAddChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Add 1 value-add energy alongside existing non-value-add → simulation energy unchanged
        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Kill entity → value-add resource harvested, simulation goes from 4 to 5
        var v3 = new SimulationEntity(v2) { Status = SimulationEntityStatus.Deceased };
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_ReflectEntityNonValueAddChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Change entity to 3 non-value-add energy → simulation goes from 4 to 2
        var v2 = new SimulationEntity("Organism");
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(2m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Change entity to 2 non-value-add energy → simulation resources unchanged, waste goes up by 1
        var v3 = new SimulationEntity("Organism");
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(2m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_ReflectSimultaneousDeathAndResourceChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Add 3 value-add energy alongside existing non-value-add → simulation energy unchanged
        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Kill entity, simultaneously changing non-value-add energy from 1 to 2:
        // simulation goes down by 1 (extra non-value-add) and up by 3 (harvested value-add) → net +2
        // no waste, since the IsValueAdd=false increase is provided by the simulation, not released
        var v3 = new SimulationEntity("Organism") { Status = SimulationEntityStatus.Deceased };
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(6m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_WasteEnergyWhenEntityLosesNonValueAdd()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity loses 1 non-value-add energy while alive → goes to waste, simulation resources unchanged
        var v3 = new SimulationEntity("Organism");
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_HarvestAbsorbsNonValueAddRelease()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity dies with 1 less non-value-add; the 1 released is absorbed into the 4 harvested → net +3
        var v3 = new SimulationEntity("Organism") { Status = SimulationEntityStatus.Deceased };
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(6m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntities_AvailableResources_HarvestPartiallyAbsorbsNonValueAddRelease()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));

        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity dies with 3 less non-value-add; 2 harvested absorbs 2 of the 3 released → 1 goes to waste
        var v3 = new SimulationEntity("Organism") { Status = SimulationEntityStatus.Deceased };
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenEntityEnergyIncreaseExceedsSimulationResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 15m, false));

        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Killing the entity and increasing non-value-add to 20 requires 10 more from simulation (only 5 available)
        var v2 = new SimulationEntity("Organism") { Status = SimulationEntityStatus.Deceased };
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 20m, false));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_EntityEnergyIncreaseSucceedsWhenHarvestCoversIt()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 15m, false));

        var v1 = new SimulationEntity("Organism");
        v1.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [v1], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        var v2 = new SimulationEntity(v1);
        v2.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v1, v2 } }, [], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Killing entity and increasing non-value-add to 20 requires 10 more, but 100 is harvested → net +90
        var v3 = new SimulationEntity("Organism") { Status = SimulationEntityStatus.Deceased };
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 20m, false));
        v3.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { v2, v3 } }, [], simulation);
        Assert.AreEqual(95m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenSameEntityAppearsMultipleTimes()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("Entity");
        var other = new SimulationEntity("Other");

        // entity appears as both an after-entity and a new entity
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(
                new Dictionary<SimulationEntity, SimulationEntity?> { { other, entity } },
                [entity],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenResultantEntitiesHaveDuplicateIndividualIds()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var id = Guid.NewGuid();
        var before1 = new SimulationEntity("Before1");
        var before2 = new SimulationEntity("Before2");
        var after1 = new SimulationEntity("After1");
        after1.IndividualId = id;
        var after2 = new SimulationEntity("After2");
        after2.IndividualId = id;

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(
                new Dictionary<SimulationEntity, SimulationEntity?> { { before1, after1 }, { before2, after2 } },
                [],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenBeforeEntityIndividualIdNotInSimulationMapping()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var before = new SimulationEntity("Before");
        before.IndividualId = Guid.NewGuid();
        // before has an IndividualId but was never registered in the simulation
        var after = new SimulationEntity("After");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(
                new Dictionary<SimulationEntity, SimulationEntity?> { { before, after } },
                [],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenBeforeEntityIndividualIdMapsToWrongEntity()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var id = Guid.NewGuid();

        var registered = new SimulationEntity("Registered");
        registered.IndividualId = id;
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [registered], simulation);

        var impostor = new SimulationEntity("Impostor");
        impostor.IndividualId = id; // same ID but different instance
        var after = new SimulationEntity("After");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(
                new Dictionary<SimulationEntity, SimulationEntity?> { { impostor, after } },
                [],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntities_Throws_WhenResultantEntityTakesExistingIndividualIdWithoutReplacingOwner()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var id = Guid.NewGuid();

        var existing = new SimulationEntity("Existing");
        existing.IndividualId = id;
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [existing], simulation);

        // afterEntity claims the same ID, but existing is not a before-entity in the mapping
        var after = new SimulationEntity("After");
        after.IndividualId = id;
        var unrelated = new SimulationEntity("Unrelated");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntities(
                new Dictionary<SimulationEntity, SimulationEntity?> { { unrelated, after } },
                [],
                simulation));
    }
}
