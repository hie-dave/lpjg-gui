using LpjGuess.Core.Models.Graphing;

namespace LpjGuess.Core.Interfaces.Graphing;

/// <summary>
/// An interface to a class which provides a palette of colours.
/// </summary>
public interface IColourPalette
{
    /// <summary>
    /// Get the colour at the given position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The colour.</returns>
    Colour GetColour(double position);
}
