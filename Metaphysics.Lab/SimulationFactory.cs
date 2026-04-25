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

        ProcessBeginningOfLifeOnEarthToGreatOxidationEvent(simulation, observerEntity);

        return simulation;
    }

    /// <summary>
    /// processes early life on Earth
    /// 1. Life agent is observed, general observer gains 1 value-add energy
    /// 2. Life created, life agent gains 1 value-add energy
    /// 3. Common life gains 1 value-add energy for the first split (bacteria/archaea)
    /// 4. End of abiogenesis era kills common life (+1 available energy to the simulation), replaced with least universal common ancesstor
    /// 5. Archaea develop methanogenesis (+1 value-add energy)
    /// 6. Bacteria evolve Type I and Type II Reaction Centers for Anoxygenic photosynthesis (+1 value-add energy)
    /// 7. Cyanobacteria evolve the oxygenic photosynthesis Z-Scheme (+1 value-add energy)
    /// 8. Bacteria+Archaea value-add energy (+2) transferred to Great Oxidation Event Casualties.
    /// 9. Great Oxidation Event kills the casualities (+2 available energy to the simulation)
    /// </summary>
    /// <param name="simulation">simulation</param>
    /// <param name="observerEntity">observer entity</param>
    private static void ProcessBeginningOfLifeOnEarthToGreatOxidationEvent(Simulation simulation, SimulationEntity observerEntity)
    {
        IReadOnlyList<SimulationResource> baseSimulationAvailableResources = new List<SimulationResource>(simulation.AvailableResources);
        IReadOnlyList<SimulationResource> baseSimulationEntityResources = simulation.GetTotalEntityResources();

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
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 0, 0, 1);

        PreLifePathway(simulation, prebioticEnvironment);

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
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 0, 0, 2);

        AddCommonLifeFeaturesPriorToArchaeaBacteriaDivergence(simulation, commonLife);

        //Archaea and bacteria diverge (~3.7–3.5 billion years ago)
        //The leading explanation is membrane divergence (ether [archaea] vs ester [bacteria] lipids, different glycerol chirality)
        Console.WriteLine("Bacteria and Archaea diverge.");
        SimulationEntity archaea = new SimulationEntity(commonLife, false, false, "Archaea");
        archaea.SetAncestor(commonLife);
        SimulationEntity bacteria = new SimulationEntity(commonLife, false, false, "Bacteria");
        bacteria.SetAncestor(commonLife);
        var divergenceResourceChange = new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        divergenceResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = archaea, ChangeType = SimulationEntityChangeType.EntityNew },
                new SimulationEntityChange { Entity = bacteria, ChangeType = SimulationEntityChangeType.EntityNew },
                divergenceResourceChange
            ],
            simulation);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 0, 0, 3);

        //In principle, methanogenesis developed prior to the bacteria/archaea split, but ended up exclusive to archaea
        Console.WriteLine("Methanogenesis is exclusive to archaea");
        AddConcept(simulation, archaea, ["Methanogenesis"], 1);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 0, 0, 4);

        //End of abiogenesis era (~3.5 bya)
        Console.WriteLine("End of abiogenesis era (~3.5 bya)");
        var luca = new SimulationEntity("Last Universal Common Ancestor");
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = luca, ChangeType = SimulationEntityChangeType.EntityNew, ReplaceEntity = commonLife },
                new SimulationEntityChange { Entity = commonLife, ChangeType = SimulationEntityChangeType.EntityKill }
            ],
            simulation);
        commonLife = luca;
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 1, 0, 3);

        //~3.5 bya
        //Single, simple reaction center
        //Used light to excite electrons and drive cyclic electron flow -> ATP
        //Starting point for all photosynthesis
        Console.WriteLine("Ancestral anoxygenic photosynthesis photosystem");
        AddConcept(simulation, bacteria, ["Ancestral photosystem"], 0);

        //~3.5-3.3 bya
        //Type I reaction center: Strong proton gradient generation, cyclic electron flow dominant
        //Type II reaction center: Strong reducing power, can reduce ferredoxin or NAD(P)+ (directly or easily)
        Console.WriteLine("Anoxygenic Photosynthesis Type I and II Reaction Centers");
        AddConcept(simulation, bacteria, ["Anoxygenic photosynthesis Type II Reaction Center", "Anoxygenic photosynthesis Type I Reaction Center"], 1);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 1, 0, 4);

        //common in bacteria (e.g. proteobacteria, chlorobi), present in some groups of archaea (e.g. thermophiles)
        //H₂S → S⁰ / SO₄²⁻. Emerged ~3.5–3.2, expands significantly by ~3.0 bya
        Console.WriteLine("Emergence of sulfur oxidation chemotrophy pathway");
        AddConcept(simulation, commonLife, ["Sulfur Oxidation Chemotrophy Pathway"], 0);

        //~3.5–3.2 bya. Chlorobi (green sulfur bacteria), some Protecobacteria (Purple sulfur bacteria)
        //CO₂ + H₂S + light → biomass + S⁰
        Console.WriteLine("Emergence of sulfur phototrophy");
        AddConcept(simulation, bacteria, ["Sulfur-based Phototrophy (Anoyxygenic Photosynthesis)"], 0);

        //Purple bacteria, some green bacteria
        //~3.3–3.0 bya
        //CO₂ + H₂ + light → organic matter
        //Works well in environments with hydrothermal or volcanic hydrogen
        Console.WriteLine("Emergence of hydrogen-based photogrophy");
        AddConcept(simulation, bacteria, ["Hydrogen-based Phototrophy (Anoyxygenic Photosynthesis)"], 0);

        //~3.3–3.0 bya
        //Purple non-sulfur bacteria, some green non-sulfur bacteria
        //use light for energy, but don’t rely on CO₂ fixation
        Console.WriteLine("Emergecy of organic compound-based phototrophy");
        AddConcept(simulation, bacteria, ["Organic comount-based Phototrophy (Anoyxygenic Photosynthesis)"], 0);

        //common in bacteria, rare/uncertain in archaea
        //emerged ~3.2–2.7 billion years ago, expanded significantly closer to ~2.7–2.4 bya as sulfur cycling became more complex
        //Example: 4S⁰ + 4H₂O → SO₄²⁻ + 3H₂S + 2H⁺
        //Requires intermediate sulfur species (like elemental sulfur, thiosulfate)
        Console.WriteLine("Emergence of sulfur disproportionation pathway");
        AddConcept(simulation, commonLife, ["Sulfur Disproportionation Pathway"], 0);

        //common in bacteria (chlorobi, proteobacteria), mostly absent in archaea
        //~3.2–2.7 bya, possibly earlier
        //Fe²⁺ + light → Fe³⁺
        Console.WriteLine("Emergence of anoxygenic phototrophic iron oxidation (photoferrotrophy)");
        AddConcept(simulation, commonLife, ["Phototrophic Iron Oxidation (Photoferrotrophy)"], 0);

        //Cyanobacterial develop (~3.0-2.7 bya)
        Console.WriteLine("Cyanobacteria develop.");
        SimulationEntity cyanobacteria = new SimulationEntity(bacteria, false, false, "Cyanobacteria");
        archaea.SetAncestor(bacteria);
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = cyanobacteria, ChangeType = SimulationEntityChangeType.EntityNew },
            ],
            simulation);

        //~3.0-2.8 bya
        //Light-driven oxidation of Mn²⁺ → Mn³⁺/Mn⁴⁺ (manganese is easier to oxidize than water, can store oxidizing equivalents)
        Console.WriteLine("Emergence of manganese-oxidizing phototrophs");
        AddConcept(simulation, cyanobacteria, ["Manganese-oxidizing phototrophy"], 0);

        //~2.9-2.7 bya
        //Binding manganese ions directly
        //Accumulation of multiple oxidizing equivalents, perfom multi-step oxidation reactions
        Console.WriteLine("Emergency of multi-manganese catalytic cluster");
        AddConcept(simulation, cyanobacteria, ["Multi-manganese catalytic cluster"], 0);

        //~2.8–2.7 bya
        //4 sequential oxidations, precise geometry, stabilization of intermediates
        Console.WriteLine("The Mn cluster becomes capable of oxidizing water");
        AddConcept(simulation, cyanobacteria, ["First water oxidation (Mn cluster)"], 0);

        //~2.7 bya
        //PSII: the "water-splitting engine"
        //Reaction center: P680. Special chlorophyll pair, Very strong oxidant (one of the strongest in biology), Can pull electrons from water
        //Water-splitting (oxygen-evolving complex). Metal cluster: Mn4CaO5, performs H2O -> O2 + H+ + e-, requires accumulation of 4 oxidizing equivalents (Kok cycle). This is the defining feature of PSII.
        //Electron transfer chain (P608 -> pheophytin -> plastoquinone (QA/QB), electrons passed to plastoquinone pool.
        //Proton gradient generation. Contributes to proton buildup in thylakoid lumen, drives ATP synthesis
        //Protein core. D1 and D2 proteins, homologous to Type II reaction center L/M proteins
        Console.WriteLine("Anoxygenic photosynthesis Photosystem II");
        AddConcept(simulation, cyanobacteria, ["Anoxygenic photosynthesis Photosystem II"], 0);

        //~2.7 bya
        //PSI: the "reducing power engine"
        //Reaction center: P700. Special chlorophyll pair, extremely srong reductant when excited
        //Electron transfer chain. P700 -> A0 -> A1 -> Fe-S clusters -> ferredoxin
        //Final step: NADP+ -> NADPH (via ferredoxin-NADP+ reductase)
        //Reducing power generation. Produces NADPH. used for CO2 fixation (Calvin cycle)
        //Protein core: PsaA and PsaB proteins, derived from Type I reaction centers
        //Can operate in two modes, linear flow (with PSII), or cyclic flow (independent ATP production)
        Console.WriteLine("Anoyxgenic photosynthesis Photosystem I");
        AddConcept(simulation, cyanobacteria, ["Anoxygenic photosystem Photosystem I"], 0);

        //~2.7–2.5 bya 
        //Flow: PSII (water -> electrons + O2), Electron transport chain, PSI (electrons -> NADPh)
        //Coupled outputs: ATP (via proton gradient, NADPH, O2)
        Console.WriteLine("Oxygenic Photosynthesis Z-Scheme");
        AddConcept(simulation, cyanobacteria, ["Oxygenic Photosynthesis Z-Scheme"], 1);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 1, 0, 5);

        //Oxygen sink: iron-rich oceans (Fe[2+] + Ox --> Fe[3+] (rust) -> sinks as rock (banded iron formations)
        //Oxygen sink: volcanic gases (H2, CH4, H2S react with oxygen)
        //Oxygen sink: Iron and sulfides in surface rock
        //Oxygen sink: O2 could be dissolved in the ocean or locally consumed
        //Cyanobacteria were geographically limited and ecologically constrained
        //~2.4 bya = Greate Oxidation Event = tipping point (all major sinks weaken simultaneously), biologically produced O2 was no longer covered by geological consumption.
        Console.WriteLine("Great Oxidation Event (~2.4 billion years ago)");
        List<SimulationEntityChange> changes = new List<SimulationEntityChange>();
        SimulationEntityChange resourceTransfer = new SimulationEntityChange { Entity = bacteria, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceTransfer.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -1, true));
        changes.Add(resourceTransfer);
        resourceTransfer = new SimulationEntityChange { Entity = archaea, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        resourceTransfer.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, -1, true));
        changes.Add(resourceTransfer);
        SimulationEntity greatOxidationEventCasualties = new SimulationEntity("Great Oxidation Event Casualties");
        greatOxidationEventCasualties.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 2m, true));
        changes.Add(new SimulationEntityChange { Entity = greatOxidationEventCasualties, ChangeType = SimulationEntityChangeType.EntityNew });
        simulation.AddOrChangeEntitiesDelta(changes, simulation);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 1, 0, 5); //no change

        //harvest resources from the great oxidation event casualties
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = greatOxidationEventCasualties, ChangeType = SimulationEntityChangeType.EntityKill }], simulation);
        ValidateSimulationResources(simulation, baseSimulationEntityResources, baseSimulationAvailableResources, 0, 3, 0, 3);
    }

    /// <summary>
    /// adds common life features prior to the archaea/bacteria divergence
    /// </summary>
    /// <param name="simulation">simulation</param>
    /// <param name="commonLife">common life entity</param>
    private static void AddCommonLifeFeaturesPriorToArchaeaBacteriaDivergence(Simulation simulation, SimulationEntity commonLife)
    {
        Console.WriteLine("Emergence of DNA as a genetic information system");
        AddConcept(simulation, commonLife, ["DNA Genetic Information System"], 0);

        //Ribosome or proto-ribosome, tRNA system
        Console.WriteLine("Emergence of protein translation system");
        AddConcept(simulation, commonLife, ["Ribosomal translation"], 0);

        Console.WriteLine("Horizontal gene transfer allows characteristics to be passed between organisms.");
        AddConcept(simulation, commonLife, ["Horizontal Gene Transfer"], 0);

        //Bacteria-dominated
        //Marginal energy yield (~ -95 kJ/mol acetate), near minimum required for life
        //2CO₂ + 4H₂ → acetate + 2H₂O
        Console.WriteLine("Acetogenesis as a optimized version of the acetyl-coa pathway");
        AddConcept(simulation, commonLife, ["Optimized acetogenesis Pathway"], 0);

        //Present in both bacteria and archae
        //High energy (~ -150 to -200+ kJ/mol), requires sulfur
        //SO₄²⁻ / S⁰ → H₂S
        Console.WriteLine("Emergence of sulfur reduction pathway");
        AddConcept(simulation, commonLife, ["Sulfur Reduction Pathway"], 0);

        //Archaea specialization branch (~3.8 to 3.5 billion years ago)
        //Moderate to high for anaerobic systems (~ -130 kJ/mol CH₄)
        //CO₂ + 4H₂ → CH₄ + 2H₂O
        Console.WriteLine("Emergence of methanogenesis pathway");
        AddConcept(simulation, commonLife, ["Methanogenesis Pathway"], 0);

        //Moderate to high energy for anaerobic systems
        //Fe³⁺ + electron donor (H₂ or organics) → Fe²⁺
        Console.WriteLine("Emergence of iron reduction pathway");
        AddConcept(simulation, commonLife, ["Iron Reduction Pathway"], 0);

        //Proton gradients + ATP synthesis
        Console.WriteLine("Emergence of chemiosmosis");
        AddConcept(simulation, commonLife, ["Chemiosmosis"], 0);
    }

    /// <summary>
    /// steps for the transition between non-life and life processes
    /// </summary>
    /// <param name="simulation">simulation</param>
    /// <param name="prebioticEnvironment">entity representing the prebiotic environent</param>
    private static void PreLifePathway(Simulation simulation, SimulationEntity prebioticEnvironment)
    {
        //~4.0–3.8 bya
        //Energy gradients + mineral catalysis (pre-cellular)
        //Natural proton gradients, abundant H₂ and CO₂, Iron-sulfur minerals acting as catalysts
        Console.WriteLine("pre-biotic environment with pre-cellular metabolism is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with pre-cellular metabolism" }], simulation);

        //~3.8–3.6 bya
        //Proto-metabolic networks
        //Self-reinforcing chemical cycles, early Acetyl-CoA-like pathway and carbon fixation
        Console.WriteLine("pre-biotic environment with pre-cellular proto-metabolic networks is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with pre-cellular proto-metabolic networks" }], simulation);

        //Compartmentalization (proto-cells) (Lipid vesicles or mineral compartments)
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with proto-cellular proto-metabolic networks" }], simulation);

        //Information systems
        //Molecules that store information and replicate (imperfectly). Likely candiate: RNA or RNA-like polymers.
        Console.WriteLine("pre-biotic environment with proto-cellular proto-metabolic networks with information is present.");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = prebioticEnvironment, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Prebiotic Environment with proto-cellular proto-metabolic networks with information" }], simulation);
    }

    private static void AddConcept(Simulation simulation, SimulationEntity entity, string[] conceptNames, decimal valueAddEnergyGain)
    {
        var conceptChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNewConcept };
        foreach (string conceptName in conceptNames)
        {
            conceptChange.Concepts.Add(new SimulationEntityConcept(conceptName));
        }
        var resourceChange = new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        if (valueAddEnergyGain > 0)
        {
            resourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, valueAddEnergyGain, true));
        }
        simulation.AddOrChangeEntitiesDelta([conceptChange, resourceChange], simulation);
    }

    private static void ValidateSimulationResources(Simulation simulation, IReadOnlyList<SimulationResource> baseEntityResources, IReadOnlyList<SimulationResource> baseAvailableSimulationResources, decimal simulationNonValueAdd, decimal simulationValueAdd, decimal entityNonValueAdd, decimal entityValueAdd)
    {
        List<SimulationResource> expectedEntityResources = new List<SimulationResource>();
        if (entityNonValueAdd > 0)
        {
            expectedEntityResources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, entityNonValueAdd, false));
        }
        if (entityValueAdd > 0)
        {
            expectedEntityResources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, entityValueAdd, true));
        }

        List<SimulationResource> expectedAvailableResources = new List<SimulationResource>();
        if (simulationNonValueAdd > 0)
        {
            expectedAvailableResources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, simulationNonValueAdd, false));
        }
        if (simulationValueAdd > 0)
        {
            expectedAvailableResources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, simulationValueAdd, true));
        }

        ValidateSimulationResources(simulation, baseEntityResources, baseAvailableSimulationResources, expectedEntityResources, expectedAvailableResources);
    }

    private static void ValidateSimulationResources(Simulation simulation, IReadOnlyList<SimulationResource> baseEntityResources, IReadOnlyList<SimulationResource> baseAvailableSimulationResources, IReadOnlyList<SimulationResource> expectedEntityResources, IReadOnlyList<SimulationResource> expectedAvailableSimulationResources)
    {
        List<SimulationResource> expectedTotalResources = new List<SimulationResource>();
        expectedTotalResources.AddRange(baseAvailableSimulationResources);
        expectedTotalResources.AddRange(expectedAvailableSimulationResources);
        if (!SimulationResource.TotalsAreEqual(expectedTotalResources, simulation.AvailableResources))
        {
            throw new InvalidOperationException("Simulation available resources mismatch");
        }
        expectedTotalResources = new List<SimulationResource>();
        expectedTotalResources.AddRange(baseEntityResources);
        expectedTotalResources.AddRange(expectedEntityResources);
        if (!SimulationResource.TotalsAreEqual(expectedTotalResources, simulation.GetTotalEntityResources()))
        {
            throw new InvalidOperationException("Simulation entity resources mismatch");
        }
    }
}
