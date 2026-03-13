using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Services;

/// <summary>
/// Interface to an instruction file parser factory, which creates parsers for
/// instruction files.
/// </summary>
public interface IInstructionFileParserFactory
{
    /// <summary>
    /// Creates an instruction file parser for the given instruction file path.
    /// </summary>
    /// <param name="instructionFilePath">The path to the instruction file.</param>
    /// <returns>An instance of <see cref="InstructionFileParser"/>.</returns>
    IInstructionFileParser Create(string instructionFilePath);
}
