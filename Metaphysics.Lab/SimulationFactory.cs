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
        List<SimulationEntity> newEntities;
        SimulationEntity entity;

        //add a general observer
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity observerEntity = new SimulationEntity("General Observer")
        {
            IsObserver = true,
        };
        observerEntity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, false));
        newEntities = new List<SimulationEntity>() { observerEntity };
        simulation.AddOrChangeEntities(mapping, newEntities, simulation);

        Console.WriteLine("Lots of physics stuff happens in the simulation.");

        //At some point an agent is observed capable of advancing life, timeline is imprecise.
        //~4.4–4.3 billion years ago: could support life but this is not widely accepted as when life actually began.
        //~4.1–3.8 billion years ago: there could be oceans, hydrothermal vents, subsurface refuges for life despite late heavy bombardment
        //By 3.8 billion years ago, life was definitely present.
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        entity = new SimulationEntity("Life Agent")
        {
            IsAgent = true,
        };
        SimulationEntity newObserver = new SimulationEntity(observerEntity);
        newObserver.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, true));
        mapping[observerEntity] = newObserver;

        //Lots of stuff happens in the simulation, but the relevant milestone is the first life forms.
        //Timeline for abiogeneis is 3.8 to 3.5 billion years ago, but possibly back to 4.1 billion years ago.
        //1. This is a simplification since there might be earlier groups of organisms that become extinct.
        //2. There are competing theories of abiogenesis, which is correct is not really relevant here.
        //2a. Heterotrophs first (using organic molecules in the environment as a resource), chemoautotrophy develops later
        //2b. Life begins as metabolic chemoautotrophy capturing naturally occuring reactions into pathway such as the acetyl-CoA pathway
        Console.WriteLine("A group of living organisms comes into existence with chemoautotrophy.");
        SimulationEntity organisms = new SimulationEntity("Primitive prokaryotic unicellular organisms with chemoautotrophy");
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [organisms], simulation);

        Console.WriteLine("This group of living organisms has staying power.");
        var nextIteration = new SimulationEntity(organisms);
        nextIteration.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { organisms, nextIteration } }, [], simulation);

        Console.WriteLine("More similar organisms come into existence.");
        organisms = new SimulationEntity("Primitive prokaryotic unicellular organisms with chemoautotrophy");
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?>(), [organisms], simulation);

        Console.WriteLine("The initial group of living organisms all die or divide, allowing their resources to be harvested by the simulation.");
        var deceasedFirstOrganisms = new SimulationEntity(nextIteration);
        deceasedFirstOrganisms.Status = SimulationEntityStatus.Deceased;
        simulation.AddOrChangeEntities(new Dictionary<SimulationEntity, SimulationEntity?> { { nextIteration, deceasedFirstOrganisms } }, [], simulation);

        return simulation;
    }
}
