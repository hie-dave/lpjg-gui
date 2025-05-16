namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// The type of line to use for a line series.
/// </summary>
/// <remarks>
/// There are actually a lot more line styles supported by oxyplot - this could
/// be expanded in the future.
/// </remarks>
public enum LineType
{
    /// <summary>
    /// A solid line.
    /// </summary>
    Solid,

    /// <summary>
    /// A dashed line.
    /// </summary>
    Dashed,

    /// <summary>
    /// A dotted line.
    /// </summary>
    Dotted
}
