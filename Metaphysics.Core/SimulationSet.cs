namespace Metaphysics.Core;

public class SimulationSet
{
    public Simulation? DevSimulation { get; set; }
    public Simulation? TestSimulation { get; set; }
    public Simulation? LiveSimulation { get; set; }

    public SimulationSet()
    {
    }
}
