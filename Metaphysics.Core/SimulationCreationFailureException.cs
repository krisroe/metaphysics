namespace Metaphysics.Core;

public class SimulationCreationFailureException : Exception
{
    public SimulationCreationFailureException() { }
    public SimulationCreationFailureException(string message) : base(message) { }
    public SimulationCreationFailureException(string message, Exception innerException) : base(message, innerException) { }
}
