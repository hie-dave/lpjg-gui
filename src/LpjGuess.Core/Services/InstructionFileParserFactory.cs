using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Services;

/// <summary>
/// Factory for creating instruction file parsers.
/// </summary>
public class InstructionFileParserFactory : IInstructionFileParserFactory
{
    /// <inheritdoc />
    public IInstructionFileParser Create(string instructionFilePath)
    {
        return InstructionFileParser.FromFile(instructionFilePath);
    }
}
