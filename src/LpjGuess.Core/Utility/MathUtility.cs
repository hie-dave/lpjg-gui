namespace LpjGuess.Core.Utility;

/// <summary>
/// Math utility functions.
/// </summary>
public static class MathUtility
{
    /// <summary>
    /// Test whether two values are equal within a tolerance.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="tolerance">The tolerance.</param>
    /// <returns>True if the values are equal within the tolerance.</returns>
    public static bool AreEqual(double a, double b, double tolerance = 1e-12)
    {
        return Math.Abs(a - b) <= tolerance;
    }
}
