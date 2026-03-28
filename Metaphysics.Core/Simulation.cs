namespace Metaphysics.Core;

public class Simulation : IDisposable
{
    private bool _disposed = false;

    public Simulation()
    {
        Console.WriteLine("Simulation beginning...");
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