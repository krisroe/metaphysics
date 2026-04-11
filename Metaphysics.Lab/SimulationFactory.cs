using Metaphysics.Core;

public static class SimulationFactory
{
    /// <summary>
    /// runs base logic for simulation maturation
    /// </summary>
    /// <returns>mature simulation</returns>
    public static Simulation RunBaseSimulationMaturation()
    {
        var simulation = new Simulation(SimulationClass.Base, intrinsicResources:
        [
            new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false),
        ]);

        Console.WriteLine("Understanding the simulation maturation process adds a unit of energy to the simulation.");
        simulation.AddAvailableResource(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, false));

        Dictionary<SimulationEntity, SimulationEntity?> mapping;
        SimulationEntity entity;

        //add a general observer
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity observerEntity = new SimulationEntity("General Observer")
        {
            IsObserver = true,
        };
        observerEntity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, false));
        simulation.AddOrChangeEntities(mapping, [observerEntity], simulation);

        Console.WriteLine("Lots of physics stuff happens in the simulation.");

        //At some point an agent is observed capable of advancing life, timeline is imprecise.
        //~4.4–4.3 billion years ago: could support life but this is not widely accepted as when life actually began.
        //~4.1–3.8 billion years ago: there could be oceans, hydrothermal vents, subsurface refuges for life despite late heavy bombardment
        //By 3.8 billion years ago, life was definitely present.
        Console.WriteLine("A life agent is observed.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity lifeAgentEntity = new SimulationEntity("Life Agent")
        {
            IsAgent = true,
        };
        observerEntity = CloneAndAddValueAddResource(observerEntity, mapping);
        simulation.AddOrChangeEntities(mapping, [lifeAgentEntity], simulation);

        //Lots of stuff happens in the simulation, but the relevant milestone is the first life forms.
        //Timeline for abiogeneis is 3.8 to 3.5 billion years ago, but possibly back to 4.1 billion years ago.
        //1. This is a simplification since there might be earlier groups of organisms that become extinct.
        //2. There are competing theories of abiogenesis, which is correct is not really relevant here.
        //2a. Heterotrophs first (using organic molecules in the environment as a resource), chemoautotrophy develops later
        //2b. Life begins as metabolic chemoautotrophy capturing naturally occuring reactions into pathway such as the acetyl-CoA pathway
        Console.WriteLine("Prokaryotic unicellular organisms with chemoautotrophy come into existence.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        entity = new SimulationEntity("Prokaryotic unicellular organisms with chemoautotrophy");
        lifeAgentEntity = CloneAndAddValueAddResource(lifeAgentEntity, mapping);
        simulation.AddOrChangeEntities(mapping, [entity], simulation);

        return simulation;
    }

    private static SimulationEntity CloneAndAddValueAddResource(SimulationEntity entity, Dictionary<SimulationEntity, SimulationEntity?> mapping)
    {
        var clone = new SimulationEntity(entity);
        clone.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, true));
        mapping[entity] = clone;
        return clone;
    }
}
