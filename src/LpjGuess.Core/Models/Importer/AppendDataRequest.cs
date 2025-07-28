namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Request model for appending data points to a layer.
/// </summary>
public class AppendDataRequest
{
    /// <summary>
    /// Data points to append.
    /// </summary>
    public IReadOnlyList<DataPoint> DataPoints { get; set; } = null!;
}
