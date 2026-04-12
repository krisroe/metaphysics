namespace Metaphysics.Core;

public enum ResourceType
{
    MetaphysicalEnergy
}

public record SimulationResource(ResourceType ResourceType, decimal Quantity, bool IsValueAdd)
{
    public decimal Quantity { get; init; } = Quantity > 0
        ? Quantity
        : throw new ArgumentOutOfRangeException(nameof(Quantity), "Quantity must be greater than zero.");
}

public record SimulationResourceDelta(ResourceType ResourceType, decimal Quantity, bool IsValueAdd);
