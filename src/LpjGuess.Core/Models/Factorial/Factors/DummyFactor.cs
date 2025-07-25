using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Core.Models.Factorial.Factors;

/// <summary>
/// A dummy factor which makes no changes.
/// </summary>
public class DummyFactor : IFactor
{
    /// <inheritdoc />
    public string GetName() => "Baseline";

    /// <inheritdoc />
    public void Apply(InstructionFileParser instructionFile) { }

    /// <inheritdoc />
    public IEnumerable<(string, string)> GetChanges() => Array.Empty<(string, string)>();
}
