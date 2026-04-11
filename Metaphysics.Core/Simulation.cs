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
        MergeResourceIntoCollection(_availableResources, resource);
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

    public void AddOrChangeEntities(Dictionary<SimulationEntity, SimulationEntity?> entityMapping, List<SimulationEntity> newEntities, Simulation originator)
    {
        var ancestors = GetAncestors();

        bool cancelled = false;

        // First pass: top-down (root to immediate parent)
        for (int i = ancestors.Count - 1; i >= 0; i--)
            ancestors[i].OnChildEntityEvent(entityMapping, newEntities, originator, i + 1, 1, ref cancelled);

        // Second pass: bottom-up (immediate parent to root)
        for (int i = 0; i < ancestors.Count; i++)
            ancestors[i].OnChildEntityEvent(entityMapping, newEntities, originator, i + 1, 2, ref cancelled);

        if (!cancelled)
        {
            if (!ValidateAddOrChangeEntitiesEvent(entityMapping, newEntities))
                throw new InvalidOperationException("AddOrChangeEntities validation failed.");

            // Compute and apply all resource changes upfront
            var (resourceChanges, wasteChanges) = ComputeEntityEventSimulationResourcesDelta(entityMapping, newEntities);

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

            // Update entity lists
            foreach (var (before, after) in entityMapping)
            {
                _entities.Remove(before);

                if (after == null)
                    continue;

                bool entityDied = before.Status == SimulationEntityStatus.Alive
                    && after.Status == SimulationEntityStatus.Deceased;

                if (!entityDied)
                    _entities.Add(after);

                if (after.IndividualId != Guid.Empty)
                    _entitiesByIndividualId[after.IndividualId] = after;
            }

            foreach (var newEntity in newEntities)
            {
                _entities.Add(newEntity);
                if (newEntity.IndividualId != Guid.Empty)
                    _entitiesByIndividualId[newEntity.IndividualId] = newEntity;
            }
        }
    }

    protected static (Dictionary<ResourceType, decimal> ResourceChanges, Dictionary<ResourceType, decimal> WasteChanges)
        ComputeEntityEventSimulationResourcesDelta(
            Dictionary<SimulationEntity, SimulationEntity?> entityMapping,
            List<SimulationEntity> newEntities)
    {
        // Compute non-value-add delta per resource type
        var nonValueAddDelta = new Dictionary<ResourceType, decimal>();

        foreach (var (before, after) in entityMapping)
        {
            IEnumerable<SimulationResource> beforeNonValueAdd = before.Resources.Where(r => !r.IsValueAdd);
            IEnumerable<SimulationResource> afterNonValueAdd = after?.Resources.Where(r => !r.IsValueAdd) ?? [];

            var allTypes = beforeNonValueAdd.Select(r => r.ResourceType)
                .Concat(afterNonValueAdd.Select(r => r.ResourceType))
                .Distinct();

            foreach (var resourceType in allTypes)
            {
                decimal afterQty = afterNonValueAdd.Where(r => r.ResourceType == resourceType).Sum(r => r.Quantity);
                decimal beforeQty = beforeNonValueAdd.Where(r => r.ResourceType == resourceType).Sum(r => r.Quantity);
                nonValueAddDelta[resourceType] = nonValueAddDelta.GetValueOrDefault(resourceType) + afterQty - beforeQty;
            }
        }

        foreach (var newEntity in newEntities)
        {
            foreach (var resourceType in newEntity.Resources.Where(r => !r.IsValueAdd).Select(r => r.ResourceType).Distinct())
            {
                decimal qty = newEntity.Resources.Where(r => !r.IsValueAdd && r.ResourceType == resourceType).Sum(r => r.Quantity);
                nonValueAddDelta[resourceType] = nonValueAddDelta.GetValueOrDefault(resourceType) + qty;
            }
        }

        // Compute harvest per resource type from deceased entities
        var harvestByType = entityMapping
            .Where(kv => kv.Key.Status == SimulationEntityStatus.Alive && kv.Value?.Status == SimulationEntityStatus.Deceased)
            .SelectMany(kv => kv.Value!.Resources.Where(r => r.IsValueAdd))
            .GroupBy(r => r.ResourceType)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        // Net non-value-add delta against harvest to produce final resource and waste changes
        var resourceChanges = new Dictionary<ResourceType, decimal>();
        var wasteChanges = new Dictionary<ResourceType, decimal>();

        foreach (var type in nonValueAddDelta.Keys.Union(harvestByType.Keys))
        {
            decimal delta = nonValueAddDelta.GetValueOrDefault(type);
            decimal harvest = harvestByType.GetValueOrDefault(type);

            if (delta >= 0)
            {
                // Entities need more (or same): harvest offsets; remainder is surplus or deficit
                decimal netNeed = delta - harvest;
                if (netNeed != 0)
                    resourceChanges[type] = -netNeed; // positive = simulation gains; negative = simulation provides
            }
            else
            {
                // Entities released resources: offset against harvest before wasting the remainder
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

    protected virtual void OnChildEntityEvent(Dictionary<SimulationEntity, SimulationEntity?> entityMapping, List<SimulationEntity> newEntities, Simulation originator, int levelsUp, int passNumber, ref bool cancelled)
    {
        if (!ValidateAddOrChangeEntitiesEvent(entityMapping, newEntities))
            cancelled = true;
    }

    protected bool ValidateAddOrChangeEntitiesEvent(Dictionary<SimulationEntity, SimulationEntity?> entityMapping, List<SimulationEntity> newEntities)
    {
        // Check that resource changes do not overdraw any simulation resource
        var (resourceChanges, _) = ComputeEntityEventSimulationResourcesDelta(entityMapping, newEntities);
        foreach (var (resourceType, change) in resourceChanges)
        {
            if (change < 0)
            {
                decimal available = _availableResources.FirstOrDefault(r => r.ResourceType == resourceType)?.Quantity ?? 0;
                if (available < -change)
                    return false;
            }
        }

        // Check for resurrection, duplicate entity references, and before-entity individual ID consistency
        var allEntities = new HashSet<SimulationEntity>();
        foreach (var (before, after) in entityMapping)
        {
            if (after != null
                && before.Status == SimulationEntityStatus.Deceased
                && after.Status == SimulationEntityStatus.Alive)
                return false;
            if (!allEntities.Add(before)) return false;
            if (after != null && !allEntities.Add(after)) return false;
            if (before.IndividualId != Guid.Empty
                && (!_entitiesByIndividualId.TryGetValue(before.IndividualId, out var mapped) || !ReferenceEquals(mapped, before)))
                return false;
        }
        foreach (var newEntity in newEntities)
            if (!allEntities.Add(newEntity)) return false;

        // Check resultant entities for duplicate individual IDs and verify any existing
        // simulation individual IDs have their owner present as a mapping input
        var resultantIds = new HashSet<Guid>();
        foreach (var after in entityMapping.Values)
        {
            if (after != null && after.IndividualId != Guid.Empty)
            {
                if (!resultantIds.Add(after.IndividualId)) return false;
                if (_entitiesByIndividualId.TryGetValue(after.IndividualId, out var existing)
                    && !entityMapping.ContainsKey(existing))
                    return false;
            }
        }
        foreach (var newEntity in newEntities)
        {
            if (newEntity.IndividualId != Guid.Empty)
            {
                if (!resultantIds.Add(newEntity.IndividualId)) return false;
                if (_entitiesByIndividualId.TryGetValue(newEntity.IndividualId, out var existing)
                    && !entityMapping.ContainsKey(existing))
                    return false;
            }
        }

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