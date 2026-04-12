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

        //add a general observer
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity observerEntity = new SimulationEntity("General Observer")
        {
            IsObserver = true,
        };
        observerEntity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, false));
        simulation.AddOrChangeEntities(mapping, [observerEntity], simulation);

        Console.WriteLine("Lots of physics stuff happens in the simulation.");

        //Environment necessary for the development of life
        //1. Chemical building blocks (CO₂, CO, CH₄, NH₃, N₂, H₂O, Phosphorus, sulfur, metals)
        //2. Persistent energy sources (Geochemical energy (H₂, redox gradients), UV radiation, Lightning / electrical discharge, Thermal gradients)
        //3. Catalytic environments (Iron-sulfur minerals, Clays, Metal ions (Fe, Ni, etc.)
        //4. Compartmentalization potential (Lipid vesicles, mineral pores)
        //5. Environmental cycling (disequilibrium) (Wet–dry cycles (shorelines), Temperature gradients, Mixing of different fluids (e.g., vents))
        //6. A balance of stability vs variability
        Console.WriteLine("pre-biotic environment with precursors for life is present.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity prebioticEnvironment = new SimulationEntity("Prebiotic Environment");
        simulation.AddOrChangeEntities(mapping, [prebioticEnvironment], simulation);

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
        observerEntity = ReplaceEntity(observerEntity, null, mapping, 1);
        simulation.AddOrChangeEntities(mapping, [lifeAgentEntity], simulation);

        //Energy gradients + mineral catalysis (pre-cellular)
        //Natural proton gradients, abundant H₂ and CO₂, Iron-sulfur minerals acting as catalysts
        Console.WriteLine("pre-biotic environment with pre-cellular metabolism is present.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        prebioticEnvironment = ReplaceEntity(prebioticEnvironment, "Prebiotic Environment with pre-cellular metabolism", mapping, 0);
        simulation.AddOrChangeEntities(mapping, new List<SimulationEntity>(), simulation);

        //Proto-metabolic networks
        //Self-reinforcing chemical cycles, early Acetyl-CoA-like pathways and carbon fixation
        Console.WriteLine("pre-biotic environment with pre-cellular proto-metabolic networks is present.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        prebioticEnvironment = ReplaceEntity(prebioticEnvironment, "Prebiotic Environment with pre-cellular proto-metabolic networks", mapping, 0);
        simulation.AddOrChangeEntities(mapping, new List<SimulationEntity>(), simulation);

        //Compartmentalization (proto-cells) (Lipid vesicles or mineral compartments)
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks is present.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        prebioticEnvironment = ReplaceEntity(prebioticEnvironment, "Prebiotic Environment with proto-cellular proto-metabolic networks", mapping, 0);
        simulation.AddOrChangeEntities(mapping, new List<SimulationEntity>(), simulation);

        //Information systems
        //Molecules that store information and replicate (imperfectly). Likely candiate: RNA or RNA-like polymers.
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks with information is present.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        prebioticEnvironment = ReplaceEntity(prebioticEnvironment, "Prebiotic Environment with proto-cellular proto-metabolic networks with information", mapping, 0);
        simulation.AddOrChangeEntities(mapping, new List<SimulationEntity>(), simulation);

        //Timeline for abiogeneis is 3.8 to 3.5 billion years ago, but possibly back to 4.1 billion years ago.
        //1. This is a simplification since there might be earlier groups of organisms that become extinct.
        //2. There are competing theories of abiogenesis, which is correct is not really relevant here.
        //2a. Heterotrophs first (using organic molecules in the environment as a resource), chemoautotrophy develops later
        //2b. Life begins as metabolic chemoautotrophy capturing naturally occuring reactions into pathway such as the acetyl-CoA pathway
        Console.WriteLine("Prokaryotic anaerobic H2-dependent, CO2 fixing proto-cellular organisms with chemoautotrophy (rudimentary acetyl-CoA-like pathway, shared metabolic system) come into existence.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity lifeContainingLUCA = ReplaceEntity(prebioticEnvironment, "Prokaryotic unicellular organisms with chemoautotrophy", mapping, 0);
        lifeAgentEntity = ReplaceEntity(lifeAgentEntity, null, mapping, 1);
        simulation.AddOrChangeEntities(mapping, new List<SimulationEntity>(), simulation);

        //Archaea and bacteria diverge (~3.7–3.5 billion years ago)
        //The leading explanation is membrane divergence (ether vs ester lipids, different glycerol chirality)
        Console.WriteLine("Bacteria and Archaea diverge.");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        SimulationEntity newLifeContainingLUCA = new SimulationEntity(lifeContainingLUCA, false, true);
        SimulationEntity archaea = new SimulationEntity(newLifeContainingLUCA, false, false, "Archaea");
        archaea.SetAncestor(newLifeContainingLUCA);
        SimulationEntity bacteria = new SimulationEntity(newLifeContainingLUCA, false, false, "Bacteria");
        bacteria.SetAncestor(newLifeContainingLUCA);
        newLifeContainingLUCA.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2, true));
        mapping[lifeContainingLUCA] = newLifeContainingLUCA;
        simulation.AddOrChangeEntities(mapping, [archaea, bacteria], simulation);

        //Methanogens develop as a branch of Archaea (~3.8 to 3.5 billion years ago)
        Console.WriteLine("Methanogens develop as a branch of Archaea");
        mapping = new Dictionary<SimulationEntity, SimulationEntity?>();
        var evolutionResult = Evolve(mapping, archaea, "Archaea Methanogens");
        simulation.AddOrChangeEntities(mapping, [evolutionResult.evolved], simulation);

        //Pre-photosynthetic pathways (Energy = moving electrons from a high-energy donor → to a lower-energy acceptor):
        //1. Acetogenesis (CO₂ + H₂ → acetate)
        //2. Methanogenesis (CO₂ + H₂ → methane), found in archaea
        //3. Sulfur reduction (SO₄²⁻ / S⁰ → H₂S)
        //4. Sulfur oxidation (H₂S → S⁰ / SO₄²⁻)
        //5. Iron oxidation (Fe²⁺ → Fe³⁺)
        //6. Iron reduction (Fe³⁺ → Fe²⁺)

        return simulation;
    }

    public static (SimulationEntity clone, SimulationEntity evolved) Evolve(
        Dictionary<SimulationEntity, SimulationEntity?> mapping,
        SimulationEntity entity,
        string evolvedName)
    {
        var clone = ReplaceEntity(entity, null, mapping, 1);
        var evolved = new SimulationEntity(entity, copyResources: false, name: evolvedName);
        evolved.SetAncestor(clone);
        return (clone, evolved);
    }

    private static SimulationEntity ReplaceEntity(SimulationEntity entity, string? newName, Dictionary<SimulationEntity, SimulationEntity?> mapping, decimal valueAdd)
    {
        SimulationEntity clone = new SimulationEntity(entity);
        if (!string.IsNullOrEmpty(newName)) clone.Name = newName;
        if (valueAdd > 0) clone.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, valueAdd, true));
        mapping[entity] = clone;
        return clone;
    }
}
