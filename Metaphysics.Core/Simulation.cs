namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;
    private readonly List<SimulationResource> _resources = new();
    private readonly List<SimulationEntity> _entities = new();
    private readonly List<SimulationSet> _simulationSets = new();

    public Simulation? Parent { get; }
    public IReadOnlyList<SimulationResource> Resources => _resources;
    public IReadOnlyList<SimulationEntity> Entities => _entities;
    public IReadOnlyList<SimulationSet> SimulationSets => _simulationSets;

    public Simulation(Simulation? parent = null)
    {
        Parent = parent;
        Console.WriteLine("Simulation beginning...");
    }

    public void AddSimulation(Simulation simulation, SimulationSet simulationSet, SimulationClass simulationClass)
    {
        var ancestors = GetAncestors();
        bool success = false;

        try
        {
            // First pass: top-down (root to immediate parent)
            for (int i = ancestors.Count - 1; i >= 0; i--)
                ancestors[i].OnChildCreateSimulation(simulation, simulationSet, simulationClass, originator: this, i + 1, 1);

            // Second pass: bottom-up (immediate parent to root)
            for (int i = 0; i < ancestors.Count; i++)
                ancestors[i].OnChildCreateSimulation(simulation, simulationSet, simulationClass, originator: this, i + 1, 2);

            success = true;
        }
        catch (SimulationCreationFailureException)
        {
        }

        if (success)
        {
            Simulation? currentSimulation = simulationClass switch
            {
                SimulationClass.Dev => simulationSet.DevSimulation,
                SimulationClass.Test => simulationSet.TestSimulation,
                SimulationClass.Live => simulationSet.LiveSimulation,
                _ => throw new SimulationCreationFailureException($"SimulationClass.{simulationClass} is not a valid class for AddSimulation.")
            };

            if (currentSimulation != null)
                throw new SimulationCreationFailureException($"{simulationClass}Simulation is already set.");

            switch (simulationClass)
            {
                case SimulationClass.Dev:
                    simulationSet.DevSimulation = simulation;
                    break;
                case SimulationClass.Test:
                    simulationSet.TestSimulation = simulation;
                    break;
                case SimulationClass.Live:
                    simulationSet.LiveSimulation = simulation;
                    break;
            }
        }
    }

    public void AddResource(SimulationResource resource)
    {
        _resources.Add(resource);
    }

    private List<Simulation> GetAncestors()
    {
        var ancestors = new List<Simulation>();
        var current = Parent;
        while (current != null)
        {
            ancestors.Add(current);
            current = current.Parent;
        }
        return ancestors;
    }

    public void AddEntity(SimulationEntity entity, Simulation originator)
    {
        var ancestors = GetAncestors();

        bool cancelled = false;

        // First pass: top-down (root to immediate parent)
        for (int i = ancestors.Count - 1; i >= 0; i--)
            ancestors[i].OnChildEntityEvent(null, entity, originator, i + 1, 1, ref cancelled);

        // Second pass: bottom-up (immediate parent to root)
        for (int i = 0; i < ancestors.Count; i++)
            ancestors[i].OnChildEntityEvent(null, entity, originator, i + 1, 2, ref cancelled);

        if (!cancelled)
            _entities.Add(entity);
    }

    protected virtual void OnChildEntityEvent(SimulationEntity? beforeEntity, SimulationEntity afterEntity, Simulation originator, int levelsUp, int passNumber, ref bool cancelled)
    {
    }

    protected virtual void OnChildCreateSimulation(Simulation simulation, SimulationSet simulationSet, SimulationClass simulationClass, Simulation originator, int levelsUp, int passNumber)
    {
        if (levelsUp == 1)
        {
            if (simulationClass == SimulationClass.Dev)
            {
                if (simulationSet.DevSimulation != null || simulationSet.TestSimulation != null || simulationSet.LiveSimulation != null)
                    throw new SimulationCreationFailureException("Cannot add a Dev simulation when the SimulationSet already has existing simulations.");
            }
            else if (simulationClass == SimulationClass.Test)
            {
                if (simulationSet.DevSimulation == null || simulationSet.TestSimulation != null || simulationSet.LiveSimulation != null)
                    throw new SimulationCreationFailureException("Cannot add a Test simulation unless Dev is set and Test and Live are unset.");
            }
            else if (simulationClass == SimulationClass.Live)
            {
                if (simulationSet.DevSimulation == null || simulationSet.TestSimulation == null || simulationSet.LiveSimulation != null)
                    throw new SimulationCreationFailureException("Cannot add a Live simulation unless Dev and Test are set and Live is unset.");
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Console.WriteLine("Simulation ending...");
            _disposed = true;
        }
    }
}