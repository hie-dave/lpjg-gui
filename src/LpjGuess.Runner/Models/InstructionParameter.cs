using System.Globalization;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Represents a parameter value in an instruction file.
/// </summary>
public class InstructionParameter
{
    private readonly string _rawValue;

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
