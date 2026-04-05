using Metaphysics.Core;
using Metaphysics.BusinessLogic;

// Individual IDs
var individualIDGenerator = new UniqueIDGenerator();

using (var simulation = new Simulation(SimulationClass.Base))
{
    Console.WriteLine("Hello, Metaphysics.");
}