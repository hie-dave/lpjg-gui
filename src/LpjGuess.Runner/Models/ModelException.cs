namespace LpjGuess.Runner.Models;

/// <summary>
/// Exception thrown when a model run fails.
/// </summary>
public class ModelException : Exception
{
    /// <summary>
    /// Create a new <see cref="ModelException"/> instance.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="modelOutput">Standard output and error from the model.</param>
    public ModelException(string message, string modelOutput) :
        base($"{message}\n\n{modelOutput}")
    { }
}
