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
    public void AddOrChangeEntity_PopulatesEntitiesByIndividualId_WhenIndividualIdIsSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");
        var id = Guid.NewGuid();
        entity.IndividualId = id;

        simulation.AddOrChangeEntity(null, entity, simulation);

        Assert.HasCount(1, simulation.EntitiesByIndividualId);
        Assert.AreEqual(entity, simulation.EntitiesByIndividualId[id]);
    }

    [TestMethod]
    public void AddOrChangeEntity_DoesNotPopulateEntitiesByIndividualId_WhenIndividualIdIsNotSet()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var entity = new SimulationEntity("TestEntity");

        simulation.AddOrChangeEntity(null, entity, simulation);

        Assert.IsEmpty(simulation.EntitiesByIndividualId);
    }

    [TestMethod]
    public void AddOrChangeEntity_ReplacesBeforeEntityWithAfterEntity_WhenBeforeEntityIsNonNull()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var before = new SimulationEntity("Before");
        simulation.AddOrChangeEntity(null, before, simulation);

        var after = new SimulationEntity("After");
        simulation.AddOrChangeEntity(before, after, simulation);

        Assert.HasCount(1, simulation.Entities);
        Assert.AreEqual(after, simulation.Entities[0]);
    }

    [TestMethod]
    public void AddOrChangeEntity_Throws_WhenChangingFromDeceasedToAlive()
    {
        using var simulation = new Simulation(SimulationClass.Base);
        var before = new SimulationEntity("Before") { Status = SimulationEntityStatus.Deceased };
        simulation.AddOrChangeEntity(null, before, simulation);

        var after = new SimulationEntity("After");

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            simulation.AddOrChangeEntity(before, after, simulation));
    }
}
