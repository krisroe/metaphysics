namespace Metaphysics.Core;

public enum SimulationEntityChangeType
{
    EntityNew,
    EntityAddOrRemoveResources,
    EntityNameChange,
    EntitySetIndividualId,
    EntityNewConcept,
    EntityKill
}

public class SimulationEntityChange
{
    public SimulationEntity Entity { get; set; }
    public SimulationEntityChangeType ChangeType { get; set; }
    public List<SimulationResourceDelta> Resources { get; } = new();
    public string? NewName { get; set; }
    public List<SimulationEntityConcept> Concepts { get; } = new();
    public Guid NewIndividualId { get; set; }
    public SimulationEntity? ReplaceEntity { get; set; }
}
