using Metaphysics.Core;

namespace Metaphysics.UnitTests;

[TestClass]
public class SimulationTests
{
    [TestMethod]
    public void Parent_IsNull_WhenNotProvided()
    {
        using var simulation = new Simulation();
        Assert.IsNull(simulation.Parent);
    }

    [TestMethod]
    public void Parent_IsSet_WhenProvided()
    {
        using var parent = new Simulation();
        using var child = new Simulation(parent);
        Assert.AreEqual(parent, child.Parent);
    }

    [TestMethod]
    public void Resources_IsEmpty_OnCreation()
    {
        using var simulation = new Simulation();
        Assert.IsEmpty(simulation.Resources);
    }

    [TestMethod]
    public void AddResource_AddsResourceToList()
    {
        using var simulation = new Simulation();
        var resource = new SimulationResource(ResourceType.MetaphysicalEnergy, 100m, false);

        simulation.AddResource(resource);

        Assert.HasCount(1, simulation.Resources);
        Assert.AreEqual(resource, simulation.Resources[0]);
    }

    [TestMethod]
    public void AddResource_SupportsMultipleResources()
    {
        using var simulation = new Simulation();
        var resource1 = new SimulationResource(ResourceType.MetaphysicalEnergy, 50m, false);
        var resource2 = new SimulationResource(ResourceType.MetaphysicalEnergy, 75m, false);

        simulation.AddResource(resource1);
        simulation.AddResource(resource2);

        Assert.HasCount(2, simulation.Resources);
    }
}
