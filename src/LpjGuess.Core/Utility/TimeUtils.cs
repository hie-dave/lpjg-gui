namespace LpjGuess.Core.Utility;

/// <summary>
/// Utility methods for time handling.
/// </summary>
public static class TimeUtils
{
    /// <summary>
    /// Formats a TimeSpan into a human-readable string.
    /// </summary>
    /// <param name="span">The TimeSpan to format.</param>
    /// <returns>A human-readable string representation of the TimeSpan.</returns>
    public static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalSeconds < 60)
            return $"{span.TotalSeconds:F1} seconds";
        
        if (span.TotalMinutes < 60)
            return $"{span.TotalMinutes:F1} minutes";
        
        if (span.TotalHours < 24)
            return $"{span.TotalHours:F1} hours";
        
        if (span.TotalDays < 30)
            return $"{span.TotalDays:F1} days";
        
        if (span.TotalDays < 365)
        {
            double months = span.TotalDays / 30;
            return $"{months:F1} months";
        }
        
        double years = span.TotalDays / 365;
        return $"{years:F1} years";
    }

    /// <summary>
    /// Get the month number from a string.
    /// </summary>
    /// <param name="name">The name of the month.</param>
    /// <returns>The month number (1-12).</returns>
    /// <exception cref="ArgumentException">Thrown if the month name is invalid.</exception>
    public static int GetMonth(string name)
    {
        switch (name)
        {
            case "Jan": return 1;
            case "Feb": return 2;
            case "Mar": return 3;
            case "Apr": return 4;
            case "May": return 5;
            case "Jun": return 6;
            case "Jul": return 7;
            case "Aug": return 8;
            case "Sep": return 9;
            case "Oct": return 10;
            case "Nov": return 11;
            case "Dec": return 12;
            default: throw new ArgumentException($"Invalid month name: {name}");
        }
    }
}
