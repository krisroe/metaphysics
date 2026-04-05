namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;
    private readonly List<SimulationResource> _resources = new();
    private readonly List<SimulationResource> _usedUpResources = new();
    private readonly List<SimulationEntity> _entities = new();
    private readonly List<SimulationSet> _simulationSets = new();
    private readonly Dictionary<Guid, SimulationEntity> _entitiesByIndividualId = new();

    public Simulation? Parent { get; }
    public SimulationClass SimulationClass { get; }
    public IReadOnlyList<SimulationResource> Resources => _resources;
    public IReadOnlyList<SimulationResource> UsedUpResources => _usedUpResources;
    public IReadOnlyList<SimulationEntity> Entities => _entities;
    public IReadOnlyList<SimulationSet> SimulationSets => _simulationSets;
    public IReadOnlyDictionary<Guid, SimulationEntity> EntitiesByIndividualId => _entitiesByIndividualId;

    public Simulation(SimulationClass simulationClass, Simulation? parent = null)
    {
        SimulationClass = simulationClass;
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

    public void UseUpResource(SimulationResource resource)
    {
        decimal available = _resources.Where(r => r.ResourceType == resource.ResourceType).Sum(r => r.Quantity);
        if (available < resource.Quantity)
            throw new InvalidOperationException($"Insufficient {resource.ResourceType} resources. Required: {resource.Quantity}, Available: {available}.");

        decimal remaining = resource.Quantity;
        foreach (var r in _resources.Where(r => r.ResourceType == resource.ResourceType).ToList())
        {
            if (r.Quantity <= remaining)
            {
                _resources.Remove(r);
                remaining -= r.Quantity;
            }
            else
            {
                _resources.Remove(r);
                _resources.Add(new SimulationResource(r.ResourceType, r.Quantity - remaining, r.IsValueAdd));
                remaining = 0;
            }
            if (remaining == 0) break;
        }

        _usedUpResources.Add(resource);
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

    public void AddOrChangeEntity(SimulationEntity? beforeEntity, SimulationEntity afterEntity, Simulation originator)
    {
        var ancestors = GetAncestors();

        bool cancelled = false;

        // First pass: top-down (root to immediate parent)
        for (int i = ancestors.Count - 1; i >= 0; i--)
            ancestors[i].OnChildEntityEvent(beforeEntity, afterEntity, originator, i + 1, 1, ref cancelled);

        // Second pass: bottom-up (immediate parent to root)
        for (int i = 0; i < ancestors.Count; i++)
            ancestors[i].OnChildEntityEvent(beforeEntity, afterEntity, originator, i + 1, 2, ref cancelled);

        if (!cancelled)
        {
            if (beforeEntity != null
                && beforeEntity.Status == SimulationEntityStatus.Deceased
                && afterEntity.Status == SimulationEntityStatus.Alive)
                throw new InvalidOperationException("Cannot change an entity from Deceased to Alive.");

            if (beforeEntity != null)
                _entities.Remove(beforeEntity);

            bool entityDied = beforeEntity != null
                && beforeEntity.Status == SimulationEntityStatus.Alive
                && afterEntity.Status == SimulationEntityStatus.Deceased;

            if (entityDied)
            {
                foreach (var resource in afterEntity.Resources)
                {
                    if (resource.IsValueAdd)
                        _resources.Add(resource);
                    else
                        _usedUpResources.Add(resource);
                }
            }
            else
            {
                _entities.Add(afterEntity);
            }

            if (afterEntity.IndividualId != Guid.Empty)
                _entitiesByIndividualId[afterEntity.IndividualId] = afterEntity;
        }
    }

    protected virtual void OnChildEntityEvent(SimulationEntity? beforeEntity, SimulationEntity afterEntity, Simulation originator, int levelsUp, int passNumber, ref bool cancelled)
    {
        if (beforeEntity != null
            && beforeEntity.Status == SimulationEntityStatus.Deceased
            && afterEntity.Status == SimulationEntityStatus.Alive)
            cancelled = true;
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