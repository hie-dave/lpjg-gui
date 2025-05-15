namespace LpjGuess.Core.Models.Graphing;

/// <summary>
/// Represents the requirements for an axis.
/// </summary>
public class AxisRequirements
{
    /// <summary>
    /// The type of axis.
    /// </summary>
    public AxisType Type { get; private init; }

    /// <summary>
    /// The position of the axis.
    /// </summary>
    public AxisPosition Position { get; private init; }

    /// <summary>
    /// The title of the axis.
    /// </summary>
    public string Title { get; private init; }

    /// <summary>
    /// Create a new <see cref="AxisRequirements"/> instance.
    /// </summary>
    /// <param name="type">The type of the axis.</param>
    /// <param name="position">The position of the axis.</param>
    /// <param name="title">The title of the axis.</param>
    public AxisRequirements(AxisType type, AxisPosition position, string title)
    {
        Type = type;
        Position = position;
        Title = title;
    }

    /// <summary>
    /// Check if these requirements are compatible with the given other
    /// requirements. Throw an exception if incompatibility is detected.
    /// </summary>
    /// <param name="other">The other requirements.</param>
    /// <exception cref="InvalidOperationException">If the axis requirements are not compatible.</exception>
    public void AssertCompatibility(AxisRequirements other)
    {
        if (Position != other.Position)
            // There can be no incompatibility between axes with different
            // positions.
            return;

        if (Type != other.Type)
            // Same position but different type. This is an invalid config.
            throw new InvalidOperationException($"Axes are incompatible: {Type} is not compatible with {other.Type}");
    }

    /// <summary>
    /// Check if these requirements represent a y-axis.
    /// </summary>
    /// <returns>True if these requirements represent a y-axis.</returns>
    public bool IsYAxis() => Position == AxisPosition.Left || Position == AxisPosition.Right;

    /// <summary>
    /// Check if these requirements represent an x-axis.
    /// </summary>
    /// <returns>True if these requirements represent an x-axis.</returns>
    public bool IsXAxis() => Position == AxisPosition.Bottom || Position == AxisPosition.Top;
}
