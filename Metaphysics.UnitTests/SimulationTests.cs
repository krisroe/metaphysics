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
    public void Resources_IsEmpty_OnCreation()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        Assert.IsEmpty(simulation.Resources);
    }

    [TestMethod]
    public void AddResource_AddsResourceToList()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var resource = new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false);

        simulation.AddResource(resource);

        Assert.HasCount(1, simulation.Resources);
        Assert.AreEqual(resource, simulation.Resources[0]);
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
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false));
        var waste = new SimulationResource(ResourceType.MetaphysicalEnergy, 40m, false);

        simulation.UseUpResource(waste);

        Assert.HasCount(1, simulation.UsedUpResources);
        Assert.AreEqual(waste, simulation.UsedUpResources[0]);
        Assert.AreEqual(60m, simulation.Resources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void UseUpResource_ConsumesAcrossMultipleEntries()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 30m, false));
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false));

        simulation.UseUpResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 70m, false));

        Assert.AreEqual(10m, simulation.Resources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void UseUpResource_Throws_WhenInsufficientResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.UseUpResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false)));
    }

    [TestMethod]
    public void AddResource_SupportsMultipleResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var resource1 = new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false);
        var resource2 = new SimulationResource(ResourceType.MetaphysicalEnergy, 75m, false);

        simulation.AddResource(resource1);
        simulation.AddResource(resource2);

        Assert.HasCount(2, simulation.Resources);
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
        var alive = new SimulationEntity("Organism");
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, true));
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [alive], simulation);

        var deceased = new SimulationEntity(alive) { Status = SimulationEntityStatus.Deceased };
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { alive, deceased } }, [], simulation);

        Assert.IsEmpty(simulation.Entities);
        Assert.HasCount(1, simulation.Resources);
        Assert.AreEqual(5m, simulation.Resources[0].Quantity);
        Assert.HasCount(1, simulation.UsedUpResources);
        Assert.AreEqual(3m, simulation.UsedUpResources[0].Quantity);
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
