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

    public string Name { get; set; }
    public SimulationEntityStatus Status { get; set; } = SimulationEntityStatus.Alive;
    public List<SimulationResource> Resources { get; } = new();
    public List<SimulationEntityConcept> Concepts { get; } = new();
    public bool IsAgent { get; set; } = false;
    public bool IsObserver { get; set; } = false;
    internal SimulationEntity? Ancestor { get; set; }
    internal List<SimulationEntity> Progeny { get; } = new();

    public void SetAncestor(SimulationEntity ancestor)
    {
        Ancestor = ancestor;
        ancestor.Progeny.Add(this);
    }

    public SimulationEntity(string name)
    {
        Name = name;
    }

    public SimulationEntity(SimulationEntity source, bool deepClone = false, bool copyResources = true, string? name = null)
    {
        Name = name ?? source.Name;
        Status = source.Status;
        if (source._individualId != Guid.Empty)
            _individualId = source._individualId;
        if (copyResources)
            Resources.AddRange(source.Resources.Select(r => r with { }));
        IsAgent = source.IsAgent;
        IsObserver = source.IsObserver;
        Ancestor = deepClone && source.Ancestor != null
            ? new SimulationEntity(source.Ancestor, deepClone: true)
            : source.Ancestor;
        if (deepClone)
            Progeny.AddRange(source.Progeny.Select(p => new SimulationEntity(p, deepClone: true)));
    }

    public override string ToString() => Name;
}
