namespace LpjGuess.Runner.Models;

/// <summary>
/// The result of a job which has been run.
/// </summary>
public sealed record JobResult(
    string Name,
    TimeSpan Duration
)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name} ({Duration})";
    }
}
