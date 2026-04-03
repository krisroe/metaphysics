namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;
    private readonly List<SimulationResource> _resources = new();
    private readonly List<SimulationEntity> _entities = new();

    public Simulation? Parent { get; }
    public IReadOnlyList<SimulationResource> Resources => _resources;
    public IReadOnlyList<SimulationEntity> Entities => _entities;

    public Simulation(Simulation? parent = null)
    {
        Parent = parent;
        Console.WriteLine("Simulation beginning...");
    }

    public void AddResource(SimulationResource resource)
    {
        _resources.Add(resource);
    }

    public void AddEntity(SimulationEntity entity, Simulation originator)
    {
        var ancestors = new List<Simulation>();
        var current = Parent;
        while (current != null)
        {
            ancestors.Add(current);
            current = current.Parent;
        }

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

    public void Dispose()
    {
        if (!_disposed)
        {
            Console.WriteLine("Simulation ending...");
            _disposed = true;
        }
    }
}