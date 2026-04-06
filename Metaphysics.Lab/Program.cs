using Metaphysics.Core;
using Metaphysics.BusinessLogic;
using Metaphysics.Lab;

if (args.Length == 0 || !Enum.TryParse<SimulationStoryType>(args[0], out var storyType))
{
    Console.WriteLine($"Usage: Metaphysics.Lab <SimulationStoryType>");
    Console.WriteLine($"Valid values: {string.Join(", ", Enum.GetNames<SimulationStoryType>())}");
    return;
}

// Individual IDs
var individualIDGenerator = new UniqueIDGenerator();

if (storyType == SimulationStoryType.HelloWorld)
{
    using (var simulation = new Simulation(SimulationClass.Base))
    {
        Console.WriteLine("Hello, Metaphysics.");
    }
}
else if (storyType == SimulationStoryType.BaseSimulationMaturation)
{
    using (var simulation = SimulationFactory.RunBaseSimulationMaturation())
    {
    }
}