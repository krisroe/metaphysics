namespace Metaphysics.Core;

public class SimulationEntityConcept
{
    public string Name { get; }
    public List<SimulationResource> Resources { get; } = new();

    public SimulationEntityConcept(string name)
    {
        Name = name;
    }
}
