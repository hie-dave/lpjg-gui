using System.Collections.Generic;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Configuration model for parameter groups in TOML input files.
/// </summary>
public class ParameterGroupConfig
{
    /// <summary>
    /// Name of the parameter group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of parameter names to their possible values.
    /// All parameters in a group must have the same number of values.
    /// </summary>
    public Dictionary<string, List<string>> Parameters { get; set; } = new();
}
