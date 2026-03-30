namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;
    private readonly List<SimulationResource> _resources = new();

    public Simulation? Parent { get; }
    public IReadOnlyList<SimulationResource> Resources => _resources;

    public Simulation(Simulation? parent = null)
    {
        Parent = parent;
        Console.WriteLine("Simulation beginning...");
    }

    public void AddResource(SimulationResource resource)
    {
        _resources.Add(resource);
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