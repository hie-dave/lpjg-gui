namespace LpjGuess.Core.Models.Factorial;

/// <summary>
/// Severity of a problem found in an experiment design.
/// </summary>
public enum ExperimentDesignIssueSeverity
{
    /// <summary>
    /// The design contains something which cannot be verified locally.
    /// </summary>
    Information,

    /// <summary>
    /// The design can run, but the user should review it.
    /// </summary>
    Warning,

    /// <summary>
    /// The design is incomplete or internally conflicting.
    /// </summary>
    Error
}

/// <summary>
/// A validation problem found in an experiment design.
/// </summary>
public sealed record ExperimentDesignIssue(
    ExperimentDesignIssueSeverity Severity,
    string Message);

/// <summary>
/// Counts and validation results for an experiment design.
/// </summary>
public sealed record ExperimentDesignAnalysis(
    long SimulationCount,
    long ModelRunCount,
    bool CountOverflowed,
    IReadOnlyList<ExperimentDesignIssue> Issues)
{
    /// <summary>
    /// Whether the design contains no validation errors.
    /// </summary>
    public bool IsValid => Issues.All(i => i.Severity != ExperimentDesignIssueSeverity.Error);
}
