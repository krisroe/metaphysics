namespace Metaphysics.Core;

public class SimulationEntity
{
    private Guid _individualId = Guid.Empty;
    public Guid IndividualId
    {
        get => _individualId;
        set
        {
            if (value == Guid.Empty)
                throw new ArgumentException("IndividualId cannot be set to an empty Guid.", nameof(value));
            if (_individualId != Guid.Empty)
                throw new InvalidOperationException("IndividualId has already been set.");
            _individualId = value;
        }
    }

    public string Name { get; }
    public List<SimulationResource> Resources { get; } = new();

    public SimulationEntity(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;
}
