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

        //add a general observer
        SimulationEntity observerEntity = new SimulationEntity("General Observer")
        {
            IsObserver = true,
        };
        observerEntity.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 1, false));
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = observerEntity, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        Console.WriteLine("Lots of physics stuff happens in the simulation.");

        //Environment necessary for the development of life
        //1. Chemical building blocks (CO₂, CO, CH₄, NH₃, N₂, H₂O, Phosphorus, sulfur, metals)
        //2. Persistent energy sources (Geochemical energy (H₂, redox gradients), UV radiation, Lightning / electrical discharge, Thermal gradients)
        //3. Catalytic environments (Iron-sulfur minerals, Clays, Metal ions (Fe, Ni, etc.)
        //4. Compartmentalization potential (Lipid vesicles, mineral pores)
        //5. Environmental cycling (disequilibrium) (Wet–dry cycles (shorelines), Temperature gradients, Mixing of different fluids (e.g., vents))
        //6. A balance of stability vs variability
        Console.WriteLine("pre-biotic environment with precursors for life is present.");
        SimulationEntity prebioticEnvironment = new SimulationEntity("Prebiotic Environment");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //At some point an agent is observed capable of advancing life, timeline is imprecise.
        //~4.4–4.3 billion years ago: could support life but this is not widely accepted as when life actually began.
        //~4.1–3.8 billion years ago: there could be oceans, hydrothermal vents, subsurface refuges for life despite late heavy bombardment
        //By 3.8 billion years ago, life was definitely present.
        Console.WriteLine("A life agent is observed.");
        SimulationEntity lifeAgentEntity = new SimulationEntity("Life Agent")
        {
            IsAgent = true,
        };
        var lifeAgentObserverResourceChange = new SimulationEntityChange { Entity = observerEntity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        lifeAgentObserverResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = lifeAgentEntity, ChangeType = SimulationEntityChangeType.EntityNew },
                lifeAgentObserverResourceChange
            ],
            simulation);

        //~4.0–3.8 bya
        //Energy gradients + mineral catalysis (pre-cellular)
        //Natural proton gradients, abundant H₂ and CO₂, Iron-sulfur minerals acting as catalysts
        Console.WriteLine("pre-biotic environment with pre-cellular metabolism is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with pre-cellular metabolism" }], simulation);

        //~3.8–3.6 bya
        //Proto-metabolic networks
        //Self-reinforcing chemical cycles, early Acetyl-CoA-like pathways and carbon fixation
        Console.WriteLine("pre-biotic environment with pre-cellular proto-metabolic networks is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with pre-cellular proto-metabolic networks" }], simulation);

        //Compartmentalization (proto-cells) (Lipid vesicles or mineral compartments)
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with proto-cellular proto-metabolic networks" }], simulation);

        //Information systems
        //Molecules that store information and replicate (imperfectly). Likely candiate: RNA or RNA-like polymers.
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks with information is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with proto-cellular proto-metabolic networks with information" }], simulation);

        //Timeline for abiogeneis is 3.8 to 3.5 billion years ago, but possibly back to 4.1 billion years ago.
        //1. This is a simplification since there might be earlier groups of organisms that become extinct.
        //2. There are competing theories of abiogenesis, which is correct is not really relevant here.
        //2a. Heterotrophs first (using organic molecules in the environment as a resource), chemoautotrophy develops later
        //2b. Life begins as metabolic chemoautotrophy capturing naturally occuring reactions into pathway such as the acetyl-CoA pathway
        Console.WriteLine("Prokaryotic anaerobic H2-dependent, CO2 fixing proto-cellular organisms with chemoautotrophy (rudimentary acetyl-CoA-like pathway, shared metabolic system) come into existence.");
        SimulationEntity commonLife = prebioticEnvironment;
        var abiogenesisLifeAgentResourceChange = new SimulationEntityChange { Entity = lifeAgentEntity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        abiogenesisLifeAgentResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prokaryotic unicellular organisms with chemoautotrophy" },
                abiogenesisLifeAgentResourceChange
            ],
            simulation);

        Console.WriteLine("Horizontal gene transfer allows characteristics to be passed between organisms.");
        var hgtConceptChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        hgtConceptChange.Concepts.Add(new SimulationEntityConcept("Horizontal Gene Transfer"));
        var hgtResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        hgtResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([hgtConceptChange, hgtResourceChange], simulation);

        //CO₂ + H₂ → acetate
        Console.WriteLine("Acetogenesis as a optimized version of the acetyl-coa pathway");
        var acetogenesisConceptChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        acetogenesisConceptChange.Concepts.Add(new SimulationEntityConcept("Acetogenesis Pathway"));
        var acetogenesisResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        acetogenesisResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([acetogenesisConceptChange, acetogenesisResourceChange], simulation);

        //SO₄²⁻ / S⁰ → H₂S
        Console.WriteLine("Emergence of sulfur reduction pathways");
        var sulfurReductionConceptChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        sulfurReductionConceptChange.Concepts.Add(new SimulationEntityConcept("Sulfur Reduction Pathways"));
        var sulfurReductionResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        sulfurReductionResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([sulfurReductionConceptChange, sulfurReductionResourceChange], simulation);

        //(Fe²⁺ → Fe³⁺) and (Fe³⁺ → Fe²⁺)
        Console.WriteLine("Emergence of iron reduction and oxidation pathways");
        var ironRedoxConceptChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        ironRedoxConceptChange.Concepts.Add(new SimulationEntityConcept("Iron Redox Pathways"));
        var ironRedoxResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        ironRedoxResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([ironRedoxConceptChange, ironRedoxResourceChange], simulation);

        //Archaea and bacteria diverge (~3.7–3.5 billion years ago)
        //The leading explanation is membrane divergence (ether vs ester lipids, different glycerol chirality)
        Console.WriteLine("Bacteria and Archaea diverge.");
        SimulationEntity archaea = new SimulationEntity(commonLife, false, false, "Archaea");
        archaea.SetAncestor(commonLife);
        SimulationEntity bacteria = new SimulationEntity(commonLife, false, false, "Bacteria");
        bacteria.SetAncestor(commonLife);
        var divergenceResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        divergenceResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 2m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = archaea, ChangeType = SimulationEntityChangeType.EntityNew },
                new SimulationEntityChange { Entity = bacteria, ChangeType = SimulationEntityChangeType.EntityNew },
                divergenceResourceChange
            ],
            simulation);

        //CO₂ + H₂ → methane
        //Methanogens develop as a branch of Archaea (~3.8 to 3.5 billion years ago)
        Console.WriteLine("Methanogens develop as a branch of Archaea");
        Evolve(simulation, archaea, "Archaea Methanogens");

        //H₂S → S⁰ / SO₄²⁻. Emerged ~3.5–3.2, expands significantly by ~3.0 bya
        Console.WriteLine("Emergence of sulfur oxidation pathways");
        var sulfurOxidationConceptChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        sulfurOxidationConceptChange.Concepts.Add(new SimulationEntityConcept("Sulfur Oxidation Pathways"));
        var sulfurOxidationResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        sulfurOxidationResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta([sulfurOxidationConceptChange, sulfurOxidationResourceChange], simulation);

        return simulation;
    }

    public static SimulationEntity Evolve(Simulation simulation, SimulationEntity entity, string evolvedName)
    {
        var evolved = new SimulationEntity(entity, copyResources: false, name: evolvedName);
        evolved.SetAncestor(entity);
        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = evolved, ChangeType = SimulationEntityChangeType.EntityNew },
                resourceChange
            ],
            simulation);
        return evolved;
    }
}
