using System.Globalization;

namespace LpjGuess.Core.Models;

/// <summary>
/// Represents a parameter value in an instruction file.
/// </summary>
public class InstructionParameter
{
    /// <summary>
    /// The raw string value of the parameter.
    /// </summary>
    private readonly string _rawValue;

    /// <summary>
    /// Create a parameter from a raw string value.
    /// </summary>
    /// <param name="rawValue">The raw string value.</param>
    public InstructionParameter(string rawValue)
    {
        // Strip any comments from the value, but respect quoted strings
        if (rawValue.StartsWith('"') && rawValue.EndsWith('"'))
        {
            _rawValue = rawValue; // Keep quoted strings intact
        }
        else
        {
            var parts = rawValue.Split('!', 2);
            _rawValue = parts[0].TrimEnd();
        }
    }

    /// <summary>
    /// Create a parameter from user input which may need to have quotes added.
    /// This function uses a series of heuristics to attempt to detect whether
    /// the input is a string or a number, and quotes the input if it's a
    /// string.
    /// </summary>
    /// <param name="userInput">The user input from which to create a parameter.</param>
    /// <returns>The created parameter.</returns>
    public static InstructionParameter FromUserInput(string userInput)
    {
        // If user explicitly provided quotes, respect them.
        if (userInput.StartsWith('"') && userInput.EndsWith('"'))
        {
            return new InstructionParameter(userInput);
        }

        // Check if it's a number. I *think* instruction files require a period
        // as the decimal separator, so we use invariant culture.
        if (double.TryParse(userInput, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            // It's a number, so store it as-is.
            return new InstructionParameter(userInput);
        }

        // If it's not a number and not pre-quoted, assume it's a string that
        // needs quoting.
        return new InstructionParameter($"\"{userInput}\"");
    }

    /// <summary>
    /// Gets the string representation of the parameter value suitable for use
    /// in an instruction file. This preserves quotes for string literals and
    /// returns other types as their raw string representation.
    /// </summary>
    /// <returns>A string suitable for use in an instruction file.</returns>
    public string ToInsFileString() => _rawValue;

    /// <summary>
    /// Gets the raw string value of the parameter.
    /// </summary>
    public string AsString() => _rawValue.Trim('"');

    /// <summary>
    /// Attempts to get the parameter value as a double.
    /// </summary>
    /// <param name="value">The parsed double value if successful.</param>
    /// <returns>True if the value could be parsed as a double, false otherwise.</returns>
    public bool TryGetDouble(out double value)
    {
        return double.TryParse(_rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Attempts to get the parameter value as an integer.
    /// </summary>
    /// <param name="value">The parsed integer value if successful.</param>
    /// <returns>True if the value could be parsed as an integer, false otherwise.</returns>
    public bool TryGetInt(out int value)
    {
        return int.TryParse(_rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Attempts to get the parameter value as an array of doubles.
    /// </summary>
    /// <param name="values">The parsed double values if successful.</param>
    /// <returns>True if all values could be parsed as doubles, false otherwise.</returns>
    public bool TryGetDoubleArray(out double[] values)
    {
        values = Array.Empty<double>();
        var valueToSplit = IsQuoted ? UnquotedString : _rawValue;
        var parts = valueToSplit.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new double[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out result[i]))
                return false;
        }

        values = result;
        return true;
    }

    /// <summary>
    /// Gets the parameter value as a double. Throws if the value cannot be parsed.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed as a double.</exception>
    public double AsDouble()
    {
        if (TryGetDouble(out var value))
            return value;
        throw new FormatException($"Cannot parse '{_rawValue}' as a double.");
    }

    /// <summary>
    /// Gets the parameter value as an integer. Throws if the value cannot be parsed.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed as an integer.</exception>
    public int AsInt()
    {
        if (TryGetInt(out var value))
            return value;
        throw new FormatException($"Cannot parse '{_rawValue}' as an integer.");
    }

    /// <summary>
    /// Gets the parameter value as an array of doubles. Throws if any value cannot be parsed.
    /// </summary>
    /// <exception cref="FormatException">Thrown when any value cannot be parsed as a double.</exception>
    public double[] AsDoubleArray()
    {
        if (TryGetDoubleArray(out var values))
            return values;
        throw new FormatException($"Cannot parse '{_rawValue}' as an array of doubles.");
    }

    /// <summary>
    /// Determines if the parameter value is a quoted string.
    /// </summary>
    public bool IsQuoted => _rawValue.StartsWith('"') && _rawValue.EndsWith('"');

    /// <summary>
    /// Gets the unquoted string value if quoted, otherwise returns the raw value.
    /// </summary>
    public string UnquotedString => IsQuoted ? _rawValue.Trim('"') : _rawValue;
}
