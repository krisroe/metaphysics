using Metaphysics.Core;
using Shouldly;

namespace Metaphysics.UnitTests;

[TestClass]
public class SimulationEntityTests
{
    [TestMethod]
    public void CloneConstructor_CopiesAllProperties()
    {
        var source = new SimulationEntity("TestEntity")
        {
            Status = SimulationEntityStatus.Deceased,
            IsAgent = true,
            IsObserver = true,
        };
        source.IndividualId = Guid.NewGuid();
        source.Resources.Add(new SimulationResource(ResourceType.MetaphysicalEnergy, 42m, true));

        var clone = new SimulationEntity(source);

        clone.ShouldSatisfyAllConditions(
            () => clone.Name.ShouldBe(source.Name),
            () => clone.Status.ShouldBe(source.Status),
            () => clone.IndividualId.ShouldBe(source.IndividualId),
            () => clone.IsAgent.ShouldBe(source.IsAgent),
            () => clone.IsObserver.ShouldBe(source.IsObserver),
            () => clone.Resources.ShouldBe(source.Resources)
        );
    }
}
