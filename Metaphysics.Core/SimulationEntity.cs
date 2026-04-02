namespace Metaphysics.Core;

public class SimulationEntity
{
    public Guid IndividualId { get; }
    public List<SimulationResource> Resources { get; } = new();

    public SimulationEntity(Guid individualId)
    {
        IndividualId = individualId;
    }
}
