using Metaphysics.Core;

public static class SimulationFactory
{
    /// <summary>
    /// runs base logic for simulation maturation
    /// </summary>
    /// <returns>mature simulation</returns>
    public static Simulation RunBaseSimulationMaturation()
    {
        var simulation = new Simulation(SimulationClass.Base);

        Console.WriteLine("One unit of resources is needed to run the simulation.");
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));

        Console.WriteLine("Understanding the simulation maturation process adds a unit of energy to the simulation.");
        simulation.AddResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));

        //Lots of stuff happens in the simulation, but the relevant milestone is the first life forms.
        //1. This is a simplification since there might be earlier groups of organisms that become extinct.
        //2. There are competing theories of abiogenesis, which is correct is not really relevant here.
        //2a. Heterotrophs first (using organic molecules in the environment as a resource), chemoautotrophy develops later
        //2b. Life begins as metabolic chemoautotrophy capturing naturally occuring reactions into pathway such as the acetyl-CoA pathway
        Console.WriteLine("A group of living organisms comes into existence with chemoautotrophy.");
        SimulationEntity organisms = new SimulationEntity("Primitive prokaryotic unicellular organisms with chemoautotrophy");
        simulation.AddOrChangeEntity(null, organisms, simulation);

        Console.WriteLine("This group of living organisms has staying power.");
        var nextIteration = new SimulationEntity(organisms);
        nextIteration.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntity(organisms, nextIteration, simulation);

        Console.WriteLine("More similar organisms come into existence.");
        organisms = new SimulationEntity("Primitive prokaryotic unicellular organisms with chemoautotrophy");
        simulation.AddOrChangeEntity(null, organisms, simulation);

        Console.WriteLine("The initial group of living organisms all die, allowing their resources to be harvested by the simulation.");
        var deceasedFirstOrganisms = new SimulationEntity(nextIteration);
        deceasedFirstOrganisms.Status = SimulationEntityStatus.Deceased;
        simulation.AddOrChangeEntity(nextIteration, deceasedFirstOrganisms, simulation);

        return simulation;
    }
}
