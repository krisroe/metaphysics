using Metaphysics.BusinessLogic;
using Metaphysics.Core;

public static class SimulationFactory
{
    /// <summary>
    /// runs base logic for simulation maturation
    /// </summary>
    /// <returns>mature simulation</returns>
    public static Simulation RunBaseSimulationMaturation(SolarSystemIceGiantStartingScenario iceGiantScenario)
    {
        UniqueIDGenerator uniqueIdGenerator = new UniqueIDGenerator();

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

        int URANUS_INDEX = 0;
        int NEPTUNE_INDEX = 1;
        int OCEANUS_INDEX = 2;

        int[] startingIceGiantOrder;
        int[] endingIceGiantOrder = [ URANUS_INDEX, NEPTUNE_INDEX ]; //modern setup
        
        switch (iceGiantScenario)
        {
            case SolarSystemIceGiantStartingScenario.UranusNeptune:
                startingIceGiantOrder = [URANUS_INDEX, NEPTUNE_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.NeptuneUranus:
                startingIceGiantOrder = [NEPTUNE_INDEX, URANUS_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.OceanusUranusNeptune:
                startingIceGiantOrder = [OCEANUS_INDEX, URANUS_INDEX, NEPTUNE_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.UranusOceanusNeptune:
                startingIceGiantOrder = [URANUS_INDEX, OCEANUS_INDEX, NEPTUNE_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.OceanusNeptuneUranus:
                startingIceGiantOrder = [OCEANUS_INDEX, NEPTUNE_INDEX, URANUS_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.NeptuneOceanusUranus:
                startingIceGiantOrder = [NEPTUNE_INDEX, OCEANUS_INDEX, URANUS_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.UranusNeptuneOceanus:
                startingIceGiantOrder = [URANUS_INDEX, NEPTUNE_INDEX, OCEANUS_INDEX];
                break;
            case SolarSystemIceGiantStartingScenario.NeptuneUranusOceanus:
                startingIceGiantOrder = [NEPTUNE_INDEX, URANUS_INDEX, OCEANUS_INDEX];
                break;
            default:
                throw new InvalidOperationException();
        }
        ProcessSolarSystemCreation(simulation, uniqueIdGenerator, startingIceGiantOrder, endingIceGiantOrder, observerEntity);

        ProcessBeginningOfLifeOnEarthToGreatOxidationEvent(simulation, observerEntity);

        return simulation;
    }

    public enum SolarSystemIceGiantStartingScenario
    {
        /// <summary>
        /// most likely 4 planet scenario
        /// </summary>
        UranusNeptune = 0,

        /// <summary>
        /// less likely 4 planet scenario
        /// </summary>
        NeptuneUranus = 1,

        /// <summary>
        /// Neptune on the outside is more common (tier 1 for 3 ice giant scenarios)
        /// </summary>
        OceanusUranusNeptune = 2,

        /// <summary>
        /// Neptune on the outside is more common (tier 1 for 3 ice giant scenarios)
        /// </summary>
        UranusOceanusNeptune = 3,

        /// <summary>
        /// uranus on the outside produces more chaos for reordering and ejections (tier 2 for 3 ice giant scenarios)
        /// </summary>
        OceanusNeptuneUranus = 4,

        /// <summary>
        /// uranus on the outside produces more chaos for reordering and ejections (tier 2 for 3 ice giant scenarios)
        /// </summary>
        NeptuneOceanusUranus = 6,

        /// <summary>
        /// tier 3 for 3 ice giant scenarios
        /// </summary>
        UranusNeptuneOceanus = 7,

        /// <summary>
        /// tier 3 for 3 ice giant scenarios
        /// </summary>
        NeptuneUranusOceanus = 8,
    }

    /// <summary>
    /// processes the formation of the solar system
    /// </summary>
    /// <param name="simulation">simulation</param>
    /// <param name="individualIdGenerator">individual id generator</param>
    /// <param name="startingIceGiantOrder">starting ice giant order from closest to the sun to farthest: 0 = proto-Uranus, 1 = proto-Neptune, 2 = proto-Oceanus</param>
    /// <param name="endingIceGiantOrder">ending ice giant order from closest to the sun to farthest: 0 = proto-Uranus, 1 = proto-Neptune, 2 = proto-Oceanus</param>
    /// <param name="observerEntity">the entity observing the solar system creation</param>
    private static void ProcessSolarSystemCreation(Simulation simulation, UniqueIDGenerator individualIdGenerator, int[] startingIceGiantOrder, int[] endingIceGiantOrder, SimulationEntity observerEntity)
    {
        int startingIceGiantCount = startingIceGiantOrder.Length;
        if (startingIceGiantCount != 2 && startingIceGiantCount != 3) throw new InvalidOperationException();
        if (!startingIceGiantOrder.Contains(0)) throw new InvalidOperationException();
        if (!startingIceGiantOrder.Contains(1)) throw new InvalidOperationException();
        if (startingIceGiantCount == 3 && !startingIceGiantOrder.Contains(2)) throw new InvalidOperationException();
        bool hasOceanus = startingIceGiantCount == 3;
        int endingIceGiantCount = endingIceGiantOrder.Length;
        if (endingIceGiantCount != 2 && endingIceGiantCount != 3) throw new InvalidOperationException();
        if (endingIceGiantCount > startingIceGiantCount) throw new InvalidOperationException();
        if (!endingIceGiantOrder.Contains(0)) throw new InvalidOperationException();
        if (!endingIceGiantOrder.Contains(1)) throw new InvalidOperationException();
        if (endingIceGiantCount == 3 && !endingIceGiantOrder.Contains(2)) throw new InvalidOperationException();
        bool ejectedOceanus = hasOceanus && endingIceGiantCount == 2;

        Guid neptuneId = individualIdGenerator.GenerateID();
        Guid uranusId = individualIdGenerator.GenerateID();
        Guid oceanusId = Guid.Empty;
        if (hasOceanus) oceanusId = individualIdGenerator.GenerateID();
        Guid jupiterId = individualIdGenerator.GenerateID();
        Guid saturnId = individualIdGenerator.GenerateID();
        Guid marsId = individualIdGenerator.GenerateID();
        Guid mercuryId = individualIdGenerator.GenerateID();
        Guid venusId = individualIdGenerator.GenerateID();
        Guid earthId = individualIdGenerator.GenerateID();
        Guid earthsMoonId = individualIdGenerator.GenerateID();
        Guid theiaId = individualIdGenerator.GenerateID();

        List<SimulationEntityChange> changes;

        //~70-75% hydrogen gas (molecular hydrogen)
        //~25% helium
        //~1-2% dust and heavier elements
        //Temperature: ~10-20 K
        //Size: Tens to hundreds of light-years across
        //Mass: up to 100,000-1,000,000 solar masses
        SimulationEntity giantMolecularCloud = new SimulationEntity("Giant molecular cloud (GMC)");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = giantMolecularCloud, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //Dense core size: ~0.05-0.1 light years
        //Mass: ~1-2 solar masses
        //Density ~10^4-10^6 particles/cm3 (much denser than the average interstellar medium)
        Console.WriteLine("Dense core forms within the giant molecular cloud");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = giantMolecularCloud, ChangeType = SimulationEntityChangeType.EntityNameChange,NewName = "Giant molecular cloud with dense core" }], simulation);

        //triggered by a nearby supernova or strong stellar winds (shock or pressure wave)
        Console.WriteLine("Collapse of molecular cloud (~4.567 bya)");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = giantMolecularCloud, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Collapsing giant molecular cloud with dense core" }], simulation);
        SimulationEntity star = giantMolecularCloud;

        //to 10,000 years after collapse, protostar forms, gravity-dominated interactions creating solar nebula
        SimulationEntity solarNebulaDisk = new SimulationEntity("Solar nebula disk");
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Protostar (Class 0) (~0.01-0.1 solar mass)" },
             new SimulationEntityChange { Entity = solarNebulaDisk, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //to 100,000 years after collapse, protostar increases its mass
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Protostar (Class 0) (~0.1-0.5 solar masses)" }], simulation);

        //100,000 to 300,000 years after collapse, differentiation of inner vs outer solar system based on frost line (inside it only rock+metal can exist, outside it ices can condense)
        SimulationEntity outerSolarSystem = solarNebulaDisk;
        SimulationEntity innerSolarSystem = new SimulationEntity("Inner solar system (up to the frost line)");
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange {  Entity = solarNebulaDisk, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Outer solar system (beyond the frost line)"},
             new SimulationEntityChange { Entity = innerSolarSystem, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //~100,000 to ~500,000 years after collapse
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Protostar (Class 1) (~0.5-0.9 solar masses) " }], simulation);

        //Most solar mass assembled (~0.5-1 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange {  Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Protostar (Class 1) (~0.9-1.0+ solar masses)"}], simulation);

        //Jupiter core formation begins (~0.5-1 Myr)
        SimulationEntity jupiter = new SimulationEntity("proto-Jupiter (~3-5 AU) (~0.1-1 Earth masses)");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //T Tauri phase (~1-10 Mya) Accretion-dominated interactions.
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "T Tauri Star (~1.0 solar masses)" }], simulation);

        //Saturn core formation begins (~1-2 Myr)
        SimulationEntity saturn = new SimulationEntity("proto-Saturm (~4-6.5 AU) (~0.1-1 Earth masses");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //It is assured Jupiter will become a planet (~1-2 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (future planet) (~3-5 AU) (~1-5 Earth masses)" }], simulation);

        //Mars core formation begins (~1-3 Myr)
        SimulationEntity mars = new SimulationEntity("proto-Mars (~1.2-1.8 AU)");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = mars, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //Jupiter reaches ~10 Earth masses, ensuring it will eventually become a gas giant planet (~1-3 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (future gas giant planet) (~3-5 AU) (~10 Earth masses)" },
             new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = jupiterId}], simulation);

        //Jupiter's envelope becomes self-gravitaging (~1-4 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (future gas giant future planet) (~3-5 AU) (~15-30 Earth masses)" }], simulation);

        //Jupiter inward-migration phase (~1-2.5 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (gas giant future planet) (~1.5-2 AU) (~15-30 Earth masses)" }], simulation);

        //Jupiter undergoes runaway gas accretion over 10,000-100,000 years (~1-5 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (gas giant future planet) (~3-5 AU) (~318 Earth masses)" }], simulation);

        //Mercury core formation begins (~1-5 Myr)
        SimulationEntity mercury = new SimulationEntity("proto-Mercury (~0.2-0.7 AU) (to ~0.2 Earth masses)");
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = mercury, ChangeType = SimulationEntityChangeType.EntityNew }], simulation);

        //Ice giant core formation begins (~2-4 Myr)
        int resonanceAULowerBound, resonanceAUUpperBound;
        if (startingIceGiantCount == 2)
        {
            resonanceAULowerBound = 10;
            resonanceAUUpperBound = 20;
        }
        else //resonance chain with an extra ice giant is more compact
        {
            resonanceAULowerBound = 9;
            resonanceAUUpperBound = 17;
        }
        IceGiant? oceanusIceGiant = null;
        IceGiant[] iceGiantsByIndex;
        IceGiant neptuneIceGiant = new IceGiant("Neptune", neptuneId, 16, 16, 17, resonanceAULowerBound, resonanceAUUpperBound, "17.1");
        IceGiant uranusIceGiant = new IceGiant("Uranus", uranusId, 14, 14, 15, resonanceAULowerBound, resonanceAUUpperBound, "14.5");
        if (hasOceanus)
        {
            //Oceanus has a wide range of possible masses
            oceanusIceGiant = new IceGiant("Oceanus", oceanusId, 19, 10, 20, resonanceAULowerBound, resonanceAUUpperBound, "~10-20");

            iceGiantsByIndex = [uranusIceGiant, neptuneIceGiant, oceanusIceGiant];
        }
        else
        {
            iceGiantsByIndex = [uranusIceGiant, neptuneIceGiant];
        }

        IceGiant[] startingIceGiants = new IceGiant[startingIceGiantCount];
        startingIceGiants[0] = iceGiantsByIndex[startingIceGiantOrder[0]];
        startingIceGiants[1] = iceGiantsByIndex[startingIceGiantOrder[1]];
        if (hasOceanus)
        {
            startingIceGiants[2] = iceGiantsByIndex[startingIceGiantOrder[2]];
        }
        IceGiant[] endingIceGiants = new IceGiant[endingIceGiantCount];
        endingIceGiants[0] = iceGiantsByIndex[endingIceGiantOrder[0]];
        endingIceGiants[1] = iceGiantsByIndex[endingIceGiantOrder[1]];
        if (endingIceGiantCount == 3)
        {
            endingIceGiants[2] = iceGiantsByIndex[endingIceGiantOrder[2]];
        }

        if (endingIceGiantCount == 3)
        {
            endingIceGiants[0].FinalDistanceFromSunInAU = "~15-20";
            endingIceGiants[1].FinalDistanceFromSunInAU = "~25-35";
            endingIceGiants[2].FinalDistanceFromSunInAU = "~40-80";
        }
        else
        {
            endingIceGiants[0].FinalDistanceFromSunInAU = "19.2";
            endingIceGiants[1].FinalDistanceFromSunInAU = "30.1";
        }

        changes = new List<SimulationEntityChange>();
        foreach (IceGiant nextIceGiant in startingIceGiants)
        {
            SimulationEntity entity = new SimulationEntity($"proto-{nextIceGiant.AfterTheFactName} (~0.1-1 Earth masses) (~5-20 AU)");
            nextIceGiant.Entity = entity;
            changes.Add(new SimulationEntityChange { Entity = entity, ChangeType = SimulationEntityChangeType.EntityNew });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Saturn reaches ~10 Earth masses, ensuring it will eventually become a gas giant planet (~2-4 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Saturn (future gas giant planet) (~4-6.5 AU) (~5-15 Earth masses)" },
             new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = saturnId}], simulation);

        //Saturn undergoes runaway gas accretion over 10,000-100,000 years (~2-4 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Saturn (gas giant future planet) (~4-6.5 AU) (~95 Earth masses)" }], simulation);

        //Saturn inward-migration phase (~2-3 Myr). There is more overlap with runaway gas accretion with Saturn than Jupiter.
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Saturn (gas giant future planet) (~1.8-2.5 AU) (~95 Earth masses)" }], simulation);

        //Saturn locks in resonance with Jupiter (~2.5-3.5 Myr)
        changes = new List<SimulationEntityChange>()
        {
            new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (gas giant future planet) (~5 AU) (~318 Earth masses), in resonance with Saturn" },
            new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Saturn (gas giant future planet) (~1.8-2.5 AU) (~95 Earth masses), in resonance with Jupiter" }
        };
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Mars mostly finishes forming (~2-5 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = mars, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Mars (future rocky planet) (~1.2-1.8 AU) (~0.08-0.10 Earth masses)" },
            new SimulationEntityChange { Entity = mars, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = marsId}], simulation);

        //Grand Tack outward migration (~3-5 Myr)
        changes = new List<SimulationEntityChange>()
        {
            new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Jupiter (gas giant future planet) (~5 AU) (~318 Earth masses), in resonance with Saturn" },
            new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Saturn (gas giant future planet) (~7-10 AU) (~95 Earth masses), in resonance with Jupiter"}
        };
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Jupiter's gas capture complete, is now a planet (~3-5 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Jupiter (gas giant planet) (~5 AU) (~318 Earth masses), in resonance with Saturn" }], simulation);

        //Ice giants reach ~10 Earth masses, but the disk is too thin to become gas giants (~4-6 Myr)
        changes = new List<SimulationEntityChange>();
        foreach (IceGiant nextIceGiant in startingIceGiants)
        {
            changes.Add(new SimulationEntityChange { Entity = nextIceGiant.Entity!, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"proto-{nextIceGiant.AfterTheFactName} (not a gas giant) (~5-10 Earth masses) (~10-20 AU)" });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Venus+Earth core formation begins (~5-10 Myr)
        SimulationEntity venus = new SimulationEntity("proto-Venus (~0.5-0.9 AU)");
        SimulationEntity earth = new SimulationEntity("proto-Earth (~0.8-1.2 AU)");
        SimulationEntity theia = new SimulationEntity("proto-Theia");
        changes = new List<SimulationEntityChange>()
        {
            new SimulationEntityChange { Entity = venus, ChangeType = SimulationEntityChangeType.EntityNew },
            new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntityNew },
            new SimulationEntityChange { Entity = theia, ChangeType = SimulationEntityChangeType.EntityNew}
        };
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Mars achieves ~95-100% of its total mass (~5-10 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = mars, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Mars (rocky planet) (~1.4-1.7 AU) (~0.10-0.11 Earth masses)" }], simulation);

        //Saturn's gas capture complete, is now a planet (~5-10 Myr)
        simulation.AddOrChangeEntitiesDelta([new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Saturn (gas giant planet) (~7-10 AU) (~95 Earth masses), in resonance with Jupiter" }], simulation);

        //Ice giants accrete most of their masses (~5-10 Myr)
        changes = new List<SimulationEntityChange>();
        foreach (IceGiant nextIceGiant in startingIceGiants)
        {
            SimulationEntity iceGiantEntity = nextIceGiant.Entity!;
            changes.Add(new SimulationEntityChange { Entity = iceGiantEntity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"proto-{nextIceGiant.AfterTheFactName} (ice giant) (~10-{nextIceGiant.MostlyAccretedEarthMasses} Earth masses) (~10-20 AU)" });
            changes.Add(new SimulationEntityChange { Entity = iceGiantEntity, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = nextIceGiant.IndividualId });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Star transitions toward main sequence (~10 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Late pre-main-sequence star (post–T Tauri), contracting toward ZAMS (~1.0 solar masses)" }], simulation);

        //Gas and ice giants lock into resonance (~5-20 Myr)
        //e.g. 5 planet (Jupiter-Saturn 3:2, Saturn-IceA 3:2, IceA-IceB 2:1, IceB: IceC 3:2)
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Uranus 3:2, Uranus-Neptune 4:3) [neptune resonance break after ~0.5-1 Myr even if strongly protected]
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Uranus 4:3, Uranus-Neptune 3:2)
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Uranus 3:2, Uranus-Neptune 3:2)
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Neptune 3:2, Neptune-Uranus 4:3)
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Neptune 4:3, Neptune-Uranus 3:2)
        //e.g. 4 planet (Jupiter-Saturn 3:2, Saturn-Neptune 3:2, Neptune-Uranus 2:1)
        //4:3 is tighter and less stable (~0.05-0.5 AU drift to break). 3:2 is more stable (~0.2-1+ AU drift to break)
        //The outer planet is susceptible to moving outward.
        //4 planet models (success rate ~<1-5%) [Uranus inside more successful than Neptune inside]. 5 planet models have ~5-10x highter success rates.
        changes = new List<SimulationEntityChange>();
        foreach (IceGiant nextIceGiant in startingIceGiants)
        {
            changes.Add(new SimulationEntityChange { Entity = nextIceGiant.Entity!, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"proto-{nextIceGiant.AfterTheFactName} (ice giant), in resonance (~10-{nextIceGiant.MostlyAccretedEarthMasses} Earth masses) (~{nextIceGiant.ResonanceDistanceAULowerBound}-{nextIceGiant.ResonanceDistanceAUUpperBound} AU)" });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Multiple rocky planets obtain most of their mass (~5-20 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = mercury, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Mercury (rocky planet) (~0.2-0.7 AU) (to ~0.1-0.3 Earth masses)" },
             new SimulationEntityChange { Entity = venus, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Venus (rocky planet) (~0.6-0.9 AU) (~0.5-0.8 Earth masses)" },
             new SimulationEntityChange { Entity = venus, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = venusId },
             new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Earth (rocky planet) (~0.8-1.2 AU) (~0.2-0.6 Earth masses)"},
             new SimulationEntityChange { Entity = theia, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Theia (planetary embyro) (~0.05-0.15 Earth masses)"}
        ], simulation);

        //Ice giant final assembly largely complete (~10-30 Myr)
        changes = new List<SimulationEntityChange>();
        foreach (IceGiant nextIceGiant in startingIceGiants)
        {
            changes.Add(new SimulationEntityChange { Entity = nextIceGiant.Entity!, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"proto-{nextIceGiant.AfterTheFactName} (ice giant), in resonance (~{nextIceGiant.NearlyAccretedEarthMassesLowerBound}-{nextIceGiant.NearlyAccretedEarthMassesUpperBound} Earth masses) (~{nextIceGiant.ResonanceDistanceAULowerBound}-{nextIceGiant.ResonanceDistanceAUUpperBound} AU)" });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);

        //Mercury giant impact / mantle stripping (~10-50 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = mercury, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Mercury (rocky planet) (~0.3-0.5 AU) (0.055 Earth masses)" },
             new SimulationEntityChange { Entity = mercury, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = mercuryId}], simulation);

        //Sun becomes main sequence star (~30-50 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = star, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Sun (Zero-age Main Sequence Star) (~0.999+ solar masses)" }], simulation);

        //Moon forming impact (~30-100 Myr)
        SimulationEntity earthsMoon = new SimulationEntity("proto-Earth's-moon");
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "proto-Earth (~0.8-1.2 AU) (~0.8-0.95 Earth masses)" },
             new SimulationEntityChange { Entity = theia, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = theiaId},
             new SimulationEntityChange { Entity = theia, ChangeType = SimulationEntityChangeType.EntityKill},
             new SimulationEntityChange { Entity = earthsMoon, ChangeType = SimulationEntityChangeType.EntityNew}], simulation);

        //Moon reaches final form within within 1000's of years (~30-100 Myr). ~98-99% of Theia's mass becomes part of Earth, ~1-2% moon.
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Earth (rocky planet) (1.0 AU) (1.0 Earth masses)" },
             new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = earthId },
             new SimulationEntityChange { Entity = earthsMoon, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Earth's moon (~0.0123 Earth masses)"},
             new SimulationEntityChange { Entity = earthsMoon, ChangeType = SimulationEntityChangeType.EntitySetIndividualId, NewIndividualId = earthsMoonId }], simulation);

        //final orbits (>100 Myr)
        simulation.AddOrChangeEntitiesDelta(
            [new SimulationEntityChange { Entity = mercury, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Mercury (rocky planet) (0.387 AU) (0.055 Earth masses)" },
             new SimulationEntityChange { Entity = venus, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Venus (rocky planet) (0.723 AU) (0.815 Earth masses)" },
             new SimulationEntityChange { Entity = earth, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Earth (rocky planet) (1.0 AU) (1.0 Earth masses)"},
             new SimulationEntityChange { Entity = mars, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Mars (rocky planet) (1.52 AU) (~0.107 Earth masses)"}             
        ], simulation);

        Console.WriteLine("A randomness manipulation entity is observed.");
        SimulationEntity randomnessManipulationEntity = new SimulationEntity("Randomness Manipulation Entity")
        {
            IsAgent = true,
        };
        var observerResourceChange = new SimulationEntityChange { Entity = observerEntity, ChangeType = SimulationEntityChangeType.EntityAddOrRemoveResources };
        observerResourceChange.Resources.Add(new SimulationResourceDelta(ResourceType.MetaphysicalEnergy, 1m, true));
        simulation.AddOrChangeEntitiesDelta(
            [
                new SimulationEntityChange { Entity = randomnessManipulationEntity, ChangeType = SimulationEntityChangeType.EntityNew },
                observerResourceChange
            ],
            simulation);

        //ice giants go out of resonance, outermost to innermost
        IceGiant ig;
        for (int i = startingIceGiantCount - 1; i >= 0; i--)
        {
            ig = startingIceGiants[i];
            simulation.AddOrChangeEntitiesDelta(
                [new SimulationEntityChange { Entity = ig.Entity!, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"proto-{ig.AfterTheFactName} (ice giant), ({ig.FinalMassInEarthMasses} Earth masses) (~{ig.ResonanceDistanceAULowerBound}-{ig.ResonanceDistanceAUUpperBound} AU)" }]
                , simulation);
        }

        //oceanus has close encounter with Jupiter and is ejected
        if (ejectedOceanus)
        {
            ig = oceanusIceGiant!;
            SimulationEntity oceanusEntity = ig.Entity!;
            simulation.AddOrChangeEntitiesDelta(
                [new SimulationEntityChange { Entity = oceanusEntity, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"Oceanus (ejected ice giant), ({ig.FinalMassInEarthMasses} Earth masses)" },
                 new SimulationEntityChange { Entity = oceanusEntity, ChangeType = SimulationEntityChangeType.EntityKill }
                ], simulation);
        }

        //gas and ice giants reach their final orbits
        changes = new List<SimulationEntityChange>()
        {
            new SimulationEntityChange { Entity = jupiter, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Jupiter (gas giant planet) (5.2 AU) (~318 Earth masses), in ~5:2 resonance with Saturn" },
            new SimulationEntityChange { Entity = saturn, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = "Saturn (gas giant planet) (9.58 AU) (~95 Earth masses), in ~5:2 resonance with Jupiter" }
        };
        foreach (IceGiant nextEndingIceGiant in endingIceGiants)
        {
            changes.Add(new SimulationEntityChange { Entity = nextEndingIceGiant.Entity!, ChangeType = SimulationEntityChangeType.EntityNameChange, NewName = $"{nextEndingIceGiant.AfterTheFactName} (ice giant), ({nextEndingIceGiant.FinalMassInEarthMasses} Earth masses), ({nextEndingIceGiant.FinalDistanceFromSunInAU} AU)" });
        }
        simulation.AddOrChangeEntitiesDelta(changes, simulation);
    }

    private class IceGiant
    {
        public IceGiant(string Name, Guid IndividualId, int MostlyAccretedEarthMassesUpperBound, int NearlyAccretedEarthMassesLowerBound, int NearlyAccretedEarthMassesUpperBound, int ResonanceDistanceAULowerBound, int ResonanceDistanceAUUpperBound, string finalMassInEarthMasses)
        {
            this.AfterTheFactName = Name;
            this.IndividualId = IndividualId;
            this.MostlyAccretedEarthMasses = MostlyAccretedEarthMassesUpperBound;
            this.NearlyAccretedEarthMassesLowerBound = NearlyAccretedEarthMassesLowerBound;
            this.NearlyAccretedEarthMassesUpperBound = NearlyAccretedEarthMassesUpperBound;
            this.ResonanceDistanceAULowerBound = ResonanceDistanceAULowerBound;
            this.ResonanceDistanceAUUpperBound = ResonanceDistanceAUUpperBound;
            this.FinalMassInEarthMasses = finalMassInEarthMasses;
            this.FinalDistanceFromSunInAU = string.Empty;
        }
        public string AfterTheFactName { get; set; }
        public Guid IndividualId { get; set; }
        public SimulationEntity? Entity { get; set; }
        public int MostlyAccretedEarthMasses { get; set; }
        public int NearlyAccretedEarthMassesLowerBound { get; set; }
        public int NearlyAccretedEarthMassesUpperBound { get; set; }
        public int ResonanceDistanceAULowerBound { get; set; }
        public int ResonanceDistanceAUUpperBound { get; set; }
        public string FinalMassInEarthMasses { get; set; }
        public string FinalDistanceFromSunInAU { get; set; }
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
