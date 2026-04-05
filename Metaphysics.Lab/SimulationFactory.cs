using Metaphysics.Core;

public static class SimulationFactory
{
    public static Simulation CreateSimulation()
    {
        var simulation = new Simulation(SimulationClass.Base);
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));

        var firstOrganism = new SimulationEntity("First primitive prokaryotic unicellular organism");
        simulation.AddOrChangeEntity(null, firstOrganism, simulation);

        var firstOrganismWithResource = new SimulationEntity(firstOrganism);
        firstOrganismWithResource.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntity(firstOrganism, firstOrganismWithResource, simulation);

        var organisms = new SimulationEntity("Primitive prokaryotic unicellular organisms");
        simulation.AddOrChangeEntity(null, organisms, simulation);

        var deceasedFirstOrganism = new SimulationEntity(firstOrganismWithResource);
        deceasedFirstOrganism.Status = SimulationEntityStatus.Deceased;
        simulation.AddOrChangeEntity(firstOrganismWithResource, deceasedFirstOrganism, simulation);

        return simulation;
    }
}
