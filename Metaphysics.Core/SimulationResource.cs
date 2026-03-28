namespace Metaphysics.Core;

public enum ResourceType
{
    MetaphysicalEnergy
}

public class SimulationResource
{
    public ResourceType ResourceType { get; }
    public decimal Quantity { get; }

    public SimulationResource(ResourceType resourceType, decimal quantity)
    {
        ResourceType = resourceType;
        Quantity = quantity;
    }
}
