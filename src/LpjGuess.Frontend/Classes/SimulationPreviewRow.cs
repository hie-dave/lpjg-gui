namespace LpjGuess.Frontend.Classes;

/// <summary>
/// One generated simulation displayed in the experiment preview.
/// </summary>
public sealed record SimulationPreviewRow(
    string Simulation,
    string Changes,
    IReadOnlyDictionary<string, string> Values)
{
    /// <summary>
    /// Get the value assigned to a target, or an em dash when unchanged.
    /// </summary>
    public string GetValue(string target)
        => Values.TryGetValue(target, out string? value) ? value : "—";
}
