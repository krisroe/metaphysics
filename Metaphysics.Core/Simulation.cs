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

    public void AddEntity(SimulationEntity entity)
    {
        Parent?.OnChildEntityEvent(null, entity, this, 1);
        _entities.Add(entity);
    }

    protected virtual void OnChildEntityEvent(SimulationEntity? beforeEntity, SimulationEntity afterEntity, Simulation originator, int levelsUp)
    {
        Parent?.OnChildEntityEvent(beforeEntity, afterEntity, originator, levelsUp + 1);
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