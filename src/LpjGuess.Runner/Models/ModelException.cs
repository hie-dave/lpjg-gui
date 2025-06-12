namespace LpjGuess.Runner.Models;

public class ModelException : Exception
{
    public ModelException(string message, string modelOutput) :
        base($"{message}\n\n{modelOutput}") { }
}
