namespace Metaphysics.Core;

public enum ResourceType
{
    MetaphysicalEnergy
}

public class SimulationResource
{
    public ResourceType ResourceType { get; }
    public decimal Quantity { get; }
    public bool IsValueAdd { get; }

    public SimulationResource(ResourceType resourceType, decimal quantity, bool isValueAdd)
    {
        ResourceType = resourceType;
        Quantity = quantity;
        IsValueAdd = isValueAdd;
    }

    public SimulationResource(SimulationResource source)
    {
        ResourceType = source.ResourceType;
        Quantity = source.Quantity;
        IsValueAdd = source.IsValueAdd;
    }
}
