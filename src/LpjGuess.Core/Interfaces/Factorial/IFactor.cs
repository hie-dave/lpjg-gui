using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a change which may be applied to an instruction file.
/// </summary>
public interface IFactor
{
    /// <summary>
    /// Get a descriptive name for the factor.
    /// </summary>
    string GetName();

    /// <summary>
    /// Apply this factor to the given instruction file.
    /// </summary>
    /// <param name="instructionFile">The instruction file to apply this factor to.</param>
    void Apply(InstructionFileParser instructionFile);

    /// <summary>
    /// Get the changes this factor makes to an instruction file.
    /// </summary>
    /// <returns>Collection of name-value pairs.</returns>
    IEnumerable<(string, string)> GetChanges();
}
