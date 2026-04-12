namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;
    private readonly List<SimulationResource> _availableResources = new();
    private readonly List<SimulationResource> _intrinsicResources = new();
    private readonly List<SimulationResource> _usedUpResources = new();
    private readonly List<SimulationEntity> _entities = new();
    private readonly List<SimulationSet> _simulationSets = new();
    private readonly Dictionary<Guid, SimulationEntity> _entitiesByIndividualId = new();

    public Simulation? Parent { get; }
    public SimulationClass SimulationClass { get; }
    public IReadOnlyList<SimulationResource> AvailableResources => _availableResources;
    public IReadOnlyList<SimulationResource> IntrinsicResources => _intrinsicResources;
    public IReadOnlyList<SimulationResource> UsedUpResources => _usedUpResources;
    public IReadOnlyList<SimulationEntity> Entities => _entities;
    public IReadOnlyList<SimulationSet> SimulationSets => _simulationSets;
    public IReadOnlyDictionary<Guid, SimulationEntity> EntitiesByIndividualId => _entitiesByIndividualId;

    public Simulation(SimulationClass simulationClass, Simulation? parent = null, IEnumerable<SimulationResource>? intrinsicResources = null)
    {
        SimulationClass = simulationClass;
        Parent = parent;
        if (intrinsicResources != null)
            foreach (var resource in intrinsicResources)
                AddIntrinsicResource(resource);
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

            if (simulation.IntrinsicResources.Count == 0)
                throw new SimulationCreationFailureException("Cannot create a simulation with no intrinsic resources.");

            foreach (var resource in simulation.IntrinsicResources)
            {
                decimal available = _availableResources.FirstOrDefault(r => r.ResourceType == resource.ResourceType)?.Quantity ?? 0;
                if (available < resource.Quantity)
                    throw new SimulationCreationFailureException($"Insufficient {resource.ResourceType} to create simulation. Required: {resource.Quantity}, Available: {available}.");
            }

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

            foreach (var resource in simulation.IntrinsicResources)
                UseUpResource(resource);
        }
    }

    public void AddAvailableResource(SimulationResource resource)
    {
        MergeResourceIntoCollection(_availableResources, resource);
    }

    public void AddIntrinsicResource(SimulationResource resource)
    {
        MergeResourceIntoCollection(_intrinsicResources, resource);
    }

    public void UseUpResource(SimulationResource resource)
    {
        var existing = _availableResources.FirstOrDefault(r => r.ResourceType == resource.ResourceType);
        decimal available = existing?.Quantity ?? 0;
        if (available < resource.Quantity)
            throw new InvalidOperationException($"Insufficient {resource.ResourceType} resources. Required: {resource.Quantity}, Available: {available}.");

        if (existing!.Quantity == resource.Quantity)
            _availableResources.Remove(existing);
        else
            _availableResources[_availableResources.IndexOf(existing)] = existing with { Quantity = existing.Quantity - resource.Quantity };

        MergeResourceIntoCollection(_usedUpResources, resource);
    }

    private static void MergeResourceIntoCollection(List<SimulationResource> collection, SimulationResource resource)
    {
        var existing = collection.FirstOrDefault(r => r.ResourceType == resource.ResourceType);
        if (existing != null)
            collection[collection.IndexOf(existing)] = existing with { Quantity = existing.Quantity + resource.Quantity };
        else
            collection.Add(resource);
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

    public void AddOrChangeEntitiesDelta(List<SimulationEntityChange> changes, Simulation originator)
    {
        var ancestors = GetAncestors();

        bool cancelled = false;

        // First pass: top-down (root to immediate parent)
        for (int i = ancestors.Count - 1; i >= 0; i--)
            ancestors[i].OnChildEntityEventDelta(changes, originator, i + 1, 1, ref cancelled);

        // Second pass: bottom-up (immediate parent to root)
        for (int i = 0; i < ancestors.Count; i++)
            ancestors[i].OnChildEntityEventDelta(changes, originator, i + 1, 2, ref cancelled);

        if (!cancelled)
        {
            if (!ValidateAddOrChangeEntitiesDeltaEvent(changes))
                throw new InvalidOperationException("AddOrChangeEntitiesDelta validation failed.");

            // Compute and apply all resource changes upfront
            var (resourceChanges, wasteChanges) = ComputeEntityEventSimulationResourcesDelta(changes);

            foreach (var (type, change) in resourceChanges)
            {
                if (change > 0)
                {
                    MergeResourceIntoCollection(_availableResources, new SimulationResource(type, change, true));
                }
                else if (change < 0)
                {
                    decimal deduction = -change;
                    var existing = _availableResources.FirstOrDefault(r => r.ResourceType == type)!;
                    if (existing.Quantity == deduction)
                        _availableResources.Remove(existing);
                    else
                        _availableResources[_availableResources.IndexOf(existing)] = existing with { Quantity = existing.Quantity - deduction };
                }
            }
            foreach (var (type, qty) in wasteChanges)
                MergeResourceIntoCollection(_usedUpResources, new SimulationResource(type, qty, false));

            // Persist entity changes
            foreach (var change in changes)
            {
                switch (change.ChangeType)
                {
                    case SimulationEntityChangeType.EntityNew:
                        _entities.Add(change.Entity);
                        if (change.Entity.IndividualId != Guid.Empty)
                            _entitiesByIndividualId[change.Entity.IndividualId] = change.Entity;
                        if (change.Entity.Ancestor != null && !change.Entity.Ancestor.Progeny.Contains(change.Entity))
                            change.Entity.Ancestor.Progeny.Add(change.Entity);
                        break;
                    case SimulationEntityChangeType.EntityNameChange:
                        change.Entity.Name = change.NewName!;
                        break;
                    case SimulationEntityChangeType.EntitySetIndividualId:
                        change.Entity.IndividualId = change.NewIndividualId;
                        if (change.NewIndividualId != Guid.Empty)
                            _entitiesByIndividualId[change.NewIndividualId] = change.Entity;
                        break;
                    case SimulationEntityChangeType.EntityAddOrRemoveResources:
                        foreach (var delta in change.Resources)
                        {
                            var existing = change.Entity.Resources.FirstOrDefault(r => r.ResourceType == delta.ResourceType && r.IsValueAdd == delta.IsValueAdd);
                            if (existing != null)
                            {
                                var newQty = existing.Quantity + delta.Quantity;
                                if (newQty == 0)
                                    change.Entity.Resources.Remove(existing);
                                else
                                    change.Entity.Resources[change.Entity.Resources.IndexOf(existing)] = existing with { Quantity = newQty };
                            }
                            else
                            {
                                change.Entity.Resources.Add(new SimulationResource(delta.ResourceType, delta.Quantity, delta.IsValueAdd));
                            }
                        }
                        break;
                    case SimulationEntityChangeType.EntityNewConcept:
                        foreach (var concept in change.Concepts)
                            change.Entity.Concepts.Add(concept);
                        break;
                    case SimulationEntityChangeType.EntityKill:
                        _entities.Remove(change.Entity);
                        break;
                }
            }
        }
    }

    private static IEnumerable<SimulationResource> AllResources(SimulationEntity entity) =>
        entity.Resources.Concat(entity.Concepts.SelectMany(c => c.Resources));

    protected static (Dictionary<ResourceType, decimal> ResourceChanges, Dictionary<ResourceType, decimal> WasteChanges)
        ComputeEntityEventSimulationResourcesDelta(List<SimulationEntityChange> changes)
    {
        var nonValueAddDelta = new Dictionary<ResourceType, decimal>();
        var harvestByType = new Dictionary<ResourceType, decimal>();

        foreach (var change in changes)
        {
            switch (change.ChangeType)
            {
                case SimulationEntityChangeType.EntityNew:
                {
                    foreach (var resource in AllResources(change.Entity).Where(r => !r.IsValueAdd))
                        nonValueAddDelta[resource.ResourceType] = nonValueAddDelta.GetValueOrDefault(resource.ResourceType) + resource.Quantity;
                    break;
                }
                case SimulationEntityChangeType.EntityAddOrRemoveResources:
                {
                    foreach (var resource in change.Resources.Where(r => !r.IsValueAdd))
                        nonValueAddDelta[resource.ResourceType] = nonValueAddDelta.GetValueOrDefault(resource.ResourceType) + resource.Quantity;
                    break;
                }
                case SimulationEntityChangeType.EntityKill:
                {
                    foreach (var resource in AllResources(change.Entity).Where(r => r.IsValueAdd))
                        harvestByType[resource.ResourceType] = harvestByType.GetValueOrDefault(resource.ResourceType) + resource.Quantity;
                    break;
                }
            }
        }

        var resourceChanges = new Dictionary<ResourceType, decimal>();
        var wasteChanges = new Dictionary<ResourceType, decimal>();

        foreach (var type in nonValueAddDelta.Keys.Union(harvestByType.Keys))
        {
            decimal delta = nonValueAddDelta.GetValueOrDefault(type);
            decimal harvest = harvestByType.GetValueOrDefault(type);

            if (delta >= 0)
            {
                decimal netNeed = delta - harvest;
                if (netNeed != 0)
                    resourceChanges[type] = -netNeed;
            }
            else
            {
                decimal released = -delta;
                decimal absorbed = Math.Min(released, harvest);
                decimal harvestSurplus = harvest - absorbed;
                decimal waste = released - absorbed;
                if (harvestSurplus > 0)
                    resourceChanges[type] = harvestSurplus;
                if (waste > 0)
                    wasteChanges[type] = waste;
            }
        }

        return (resourceChanges, wasteChanges);
    }

    protected virtual void OnChildEntityEventDelta(List<SimulationEntityChange> changes, Simulation originator, int levelsUp, int passNumber, ref bool cancelled)
    {
        if (!ValidateAddOrChangeEntitiesDeltaEvent(changes))
            cancelled = true;
    }

    protected bool ValidateAddOrChangeEntitiesDeltaEvent(List<SimulationEntityChange> changes)
    {
        // Check that resource changes do not overdraw any simulation resource
        var (resourceChanges, _) = ComputeEntityEventSimulationResourcesDelta(changes);
        foreach (var (resourceType, change) in resourceChanges)
        {
            if (change < 0)
            {
                decimal available = _availableResources.FirstOrDefault(r => r.ResourceType == resourceType)?.Quantity ?? 0;
                if (available < -change)
                    return false;
            }
        }

        // Check entity list membership: EntityNew entities must not already exist; all others must
        foreach (var change in changes)
        {
            if (change.ChangeType == SimulationEntityChangeType.EntityNew)
            {
                if (_entities.Contains(change.Entity))
                    return false;
            }
            else
            {
                if (!_entities.Contains(change.Entity))
                    return false;
            }
        }

        // Check that ancestors of new entities are already in the simulation or being simultaneously created
        var newEntitiesInEvent = changes
            .Where(c => c.ChangeType == SimulationEntityChangeType.EntityNew)
            .Select(c => c.Entity)
            .ToHashSet();
        foreach (var change in changes.Where(c => c.ChangeType == SimulationEntityChangeType.EntityNew))
        {
            var ancestor = change.Entity.Ancestor;
            if (ancestor != null && !_entities.Contains(ancestor) && !newEntitiesInEvent.Contains(ancestor))
                return false;
        }

        // Check for duplicate change types on the same entity
        var seen = new HashSet<(SimulationEntity, SimulationEntityChangeType)>();
        foreach (var change in changes)
            if (!seen.Add((change.Entity, change.ChangeType)))
                return false;

        // Check for duplicate individual IDs across EntitySetIndividualId changes and against existing registrations
        var assignedIds = new HashSet<Guid>();
        foreach (var change in changes.Where(c => c.ChangeType == SimulationEntityChangeType.EntitySetIndividualId))
        {
            if (!assignedIds.Add(change.NewIndividualId))
                return false;
            if (_entitiesByIndividualId.TryGetValue(change.NewIndividualId, out var existing) && !ReferenceEquals(existing, change.Entity))
                return false;
        }

        // TODO: additional validation checks
        return true;
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