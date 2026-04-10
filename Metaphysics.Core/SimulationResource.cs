namespace Metaphysics.Core;

public enum ResourceType
{
    MetaphysicalEnergy
}

public record SimulationResource(ResourceType ResourceType, decimal Quantity, bool IsValueAdd);
