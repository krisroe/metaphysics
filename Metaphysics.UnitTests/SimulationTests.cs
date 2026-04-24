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
        var intrinsicResource = new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false);
        using var simulation = new Simulation(SimulationClass.Base, intrinsicResources: [intrinsicResource]);
        simulation.AddIntrinsicResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));
        var availableResource = new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false);

        simulation.AddAvailableResource(availableResource);

        Assert.HasCount(1, simulation.IntrinsicResources);
        Assert.AreEqual(60m, simulation.IntrinsicResources[0].Quantity);
        Assert.HasCount(1, simulation.AvailableResources);
        Assert.AreEqual(100m, simulation.AvailableResources[0].Quantity);
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
    public void AddOrChangeEntitiesDelta_PopulatesEntitiesByIndividualId_WhenIndividualIdIsSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");
        var id = Guid.NewGuid();
        entity.IndividualId = id;

        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        Assert.HasCount(1, simulation.EntitiesByIndividualId);
        Assert.AreEqual(entity, simulation.EntitiesByIndividualId[id]);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_DoesNotPopulateEntitiesByIndividualId_WhenIndividualIdIsNotSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");

        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        Assert.IsEmpty(simulation.EntitiesByIndividualId);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_EntityNameChange_UpdatesEntityName()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("Before");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "After" }], simulation);

        Assert.HasCount(1, simulation.Entities);
        Assert.AreEqual("After", simulation.Entities[0].Name);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_OnDeath_RemovesEntityAndReallocatesResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        var alive = new SimulationEntity("Organism");
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, true));
        alive.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 3m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = alive, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = alive, ChangeType = SimulationEntityChangeType.EntityKill }], simulation);

        Assert.IsEmpty(simulation.Entities);
        Assert.HasCount(1, simulation.AvailableResources);
        Assert.AreEqual(5m, simulation.AvailableResources[0].Quantity);
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_ReflectEntityValueAddChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));
        var totalAfterAdd = simulation.GetTotalEntityResources();
        Assert.HasCount(1, totalAfterAdd);
        Assert.AreEqual(ResourceType.MetaphysicalEnergy, totalAfterAdd[0].ResourceType);
        Assert.AreEqual(1m, totalAfterAdd[0].Quantity);
        Assert.IsFalse(totalAfterAdd[0].IsValueAdd);

        // Add 1 value-add energy alongside existing non-value-add → simulation energy unchanged
        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));
        var totalAfterResourceChange = simulation.GetTotalEntityResources();
        Assert.HasCount(2, totalAfterResourceChange);
        Assert.AreEqual(1m, totalAfterResourceChange.Single(r => !r.IsValueAdd).Quantity);
        Assert.AreEqual(1m, totalAfterResourceChange.Single(r => r.IsValueAdd).Quantity);

        // Kill entity → value-add resource harvested, simulation goes from 4 to 5
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.GetTotalEntityResources());
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_ReflectEntityNonValueAddChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Add 2 non-value-add energy → entity goes from 1 to 3, simulation goes from 4 to 2
        var resourceChange1 = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange1.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntitiesDelta([resourceChange1], simulation);
        Assert.AreEqual(2m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Remove 1 non-value-add energy → entity goes from 3 to 2, simulation resources unchanged, waste goes up by 1
        var resourceChange2 = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange2.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -1m, false));
        simulation.AddOrChangeEntitiesDelta([resourceChange2], simulation);
        Assert.AreEqual(2m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_ReflectSimultaneousDeathAndResourceChange()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        // Create entity with 1 non-value-add energy → simulation goes from 5 to 4
        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Add 3 value-add energy → simulation energy unchanged
        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 3m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange], simulation);
        Assert.AreEqual(4m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Kill entity, simultaneously adding 1 non-value-add energy:
        // simulation goes down by 1 (extra non-value-add) and up by 3 (harvested value-add) → net +2
        // no waste, since the non-value-add increase is provided by the simulation, not released
        var deathResourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        deathResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddOrChangeEntitiesDelta(
            [deathResourceChange, new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }],
            simulation);
        Assert.AreEqual(6m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_WasteEnergyWhenEntityLosesNonValueAdd()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        var resourceChange1 = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange1.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange1], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity loses 1 non-value-add energy while alive → goes to waste, simulation resources unchanged
        var resourceChange2 = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange2.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -1m, false));
        simulation.AddOrChangeEntitiesDelta([resourceChange2], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_HarvestAbsorbsNonValueAddRelease()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));

        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 4m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange], simulation);
        Assert.AreEqual(3m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity dies with 1 less non-value-add; the 1 released is absorbed into the 4 harvested → net +3
        var deathResourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        deathResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -1m, false));
        simulation.AddOrChangeEntitiesDelta(
            [deathResourceChange, new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }],
            simulation);
        Assert.AreEqual(6m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_AvailableResources_HarvestPartiallyAbsorbsNonValueAddRelease()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));

        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 5m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 2m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Entity dies with 3 less non-value-add; 2 harvested absorbs 2 of the 3 released → 1 goes to waste
        var deathResourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        deathResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -3m, false));
        simulation.AddOrChangeEntitiesDelta(
            [deathResourceChange, new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }],
            simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.AreEqual(1m, simulation.UsedUpResources.Sum(r => r.Quantity));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenEntityEnergyIncreaseExceedsSimulationResources()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 15m, false));

        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Killing the entity and increasing non-value-add by 10 requires 10 more from simulation (only 5 available)
        var deathResourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        deathResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 10m, false));
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [deathResourceChange, new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_EntityEnergyIncreaseSucceedsWhenHarvestCoversIt()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 15m, false));

        var entity = new SimulationEntity("Organism");
        entity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 10m, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 100m, true));
        simulation.AddOrChangeEntitiesDelta([resourceChange], simulation);
        Assert.AreEqual(5m, simulation.AvailableResources.Sum(r => r.Quantity));

        // Killing entity and increasing non-value-add by 10 requires 10 more, but 100 is harvested → net +90
        var deathResourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        deathResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 10m, false));
        simulation.AddOrChangeEntitiesDelta(
            [deathResourceChange, new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityKill }],
            simulation);
        Assert.AreEqual(95m, simulation.AvailableResources.Sum(r => r.Quantity));
        Assert.IsEmpty(simulation.UsedUpResources);
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenSameEntityAndChangeTypeAppearMultipleTimes()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("Entity");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [
                    new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew },
                    new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }
                ],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenDuplicateEntityNameChangeAfterEntityNew()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("Entity");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [
                    new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "First" },
                    new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Second" }
                ],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenTwoEntitiesAssignedSameIndividualIdInOneEvent()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var id = Guid.NewGuid();
        var entity1 = new SimulationEntity("Entity1");
        var entity2 = new SimulationEntity("Entity2");
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = entity1, ChangeType = SimulationEntityChangeType.EntityNew },
                new SimulationEntityChange { Entity = entity2, ChangeType = SimulationEntityChangeType.EntityNew }
            ],
            simulation);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [
                    new SimulationEntityChange { Entity = entity1, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = id },
                    new SimulationEntityChange { Entity = entity2, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = id }
                ],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenAssigningIndividualIdAlreadyOwnedByAnotherEntity()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var id = Guid.NewGuid();
        var entity1 = new SimulationEntity("Entity1");
        entity1.IndividualId = id;
        var entity2 = new SimulationEntity("Entity2");
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = entity1, ChangeType = SimulationEntityChangeType.EntityNew },
                new SimulationEntityChange { Entity = entity2, ChangeType = SimulationEntityChangeType.EntityNew }
            ],
            simulation);

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [new SimulationEntityChange { Entity = entity2, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = id }],
                simulation));
    }

    [TestMethod]
    public void AddOrChangeEntitiesDelta_Throws_WhenEntityNotInSimulation()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("Entity");
        // entity was never added to the simulation

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntitiesDelta(
                [new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "NewName" }],
                simulation));
    }

}
