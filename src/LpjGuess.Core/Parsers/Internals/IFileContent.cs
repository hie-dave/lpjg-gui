namespace LpjGuess.Core.Parsers;

public partial class InstructionFileParser
{
    /// <summary>
    /// Interface to a parsed piece of a file, which can be roundtripped via
    /// <see cref="ToInsFileString(string)"/> back to an equivalent instruction
    /// file fragment.
    /// </summary>
    private interface IFileContent
    {
        /// <summary>
        /// Convert this file content back to an equivalent instruction file
        /// fragment.
        /// </summary>
        /// <param name="lineEnding">The line ending to use.</param>
        /// <returns>The file content as a string.</returns>
        string ToInsFileString(string lineEnding);
    }
}
