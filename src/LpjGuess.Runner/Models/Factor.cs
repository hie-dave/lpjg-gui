using System.Globalization;

namespace LpjGuess.Runner.Models;

/// <summary>
/// A name/value pair 
/// </summary>
public class Factor
{
	/// <summary>
	/// Name of the parameter as it appears in the instruction file.
	/// </summary>
	public string Name { get; private init; }

	/// <summary>
	/// Value of the parameter.
	/// </summary>
	public string Value { get; private init; }

	/// <summary>
	/// Create a new <see cref="Factor"/> instance.
	/// </summary>
	/// <param name="name">Name of the parameter as it appears in the instruction file.</param>
	/// <param name="value">Value of the parameter.</param>
	public Factor(string name, string value)
	{
		Name = name;
		Value = value;
	}

	/// <summary>
	/// Get a unique identifier for this factor.
	/// </sumary>
	public string GetName()
	{
		return $"{Name}_{GetShortName()}";
	}

	/// <summary>
	/// Get a non-unique identifier for this factor.
	/// </summary>
	public string GetShortName()
	{
		// Check if it's a number. If it is, we want to preserve the period,
		// because decimal point. If it's not, we don't want to preserve the
		// period (if there is one), because it may well be a file extension.
		if (double.TryParse(Value, CultureInfo.InvariantCulture, out _))
			return Value;
		return Path.GetFileNameWithoutExtension(Value);
	}
}
