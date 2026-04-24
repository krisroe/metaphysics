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

    public static bool TotalsAreEqual(IEnumerable<SimulationResource> a, IEnumerable<SimulationResource> b)
    {
        static Dictionary<(ResourceType, bool), decimal> Totals(IEnumerable<SimulationResource> list) =>
            list.GroupBy(r => (r.ResourceType, r.IsValueAdd))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));

        var totalsA = Totals(a);
        var totalsB = Totals(b);

        if (totalsA.Count != totalsB.Count) return false;
        foreach (var (key, qty) in totalsA)
            if (!totalsB.TryGetValue(key, out var other) || other != qty) return false;
        return true;
    }
}

public record SimulationResourceDelta(ResourceType ResourceType, decimal Quantity, bool IsValueAdd);
