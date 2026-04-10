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
    public SimulationEntityStatus Status { get; set; } = SimulationEntityStatus.Alive;
    public List<SimulationResource> Resources { get; } = new();
    public bool IsAgent { get; set; } = false;
    public bool IsObserver { get; set; } = false;

    public SimulationEntity(string name)
    {
        Name = name;
    }

    public SimulationEntity(SimulationEntity source)
    {
        Name = source.Name;
        Status = source.Status;
        if (source._individualId != Guid.Empty)
            _individualId = source._individualId;
        Resources.AddRange(source.Resources.Select(r => r with { }));
        IsAgent = source.IsAgent;
        IsObserver = source.IsObserver;
    }

    public override string ToString() => Name;
}
