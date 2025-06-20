namespace LpjGuess.Runner.Parsers;

public partial class InstructionFileParser
{
    /// <summary>
    /// Represents a verbatim line of text from the instruction file.
    /// </summary>
    private class VerbatimLine : IFileContent
    {
        /// <summary>
        /// The line of text.
        /// </summary>
        private readonly string line;

        /// <summary>
        /// The line number.
        /// </summary>
        private readonly int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerbatimLine"/> class.
        /// </summary>
        /// <param name="line">The line of text.</param>
        /// <param name="lineNumber">The line number.</param>
        public VerbatimLine(string line, int lineNumber)
        {
            this.line = line;
            this.lineNumber = lineNumber;
        }

        /// <inheritdoc/>
        public string ToInsFileString(string lineEnding) => $"{line}{lineEnding}";
    }
}