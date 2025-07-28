using System.Text;
using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

public partial class InstructionFileParser
{
    /// <summary>
    /// A block from an instruction file.
    /// </summary>
    private class Block : IFileContent
    {
        /// <summary>
        /// The type of the block (e.g. "group", "pft", etc).
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The name of the block (e.g. "common", "TrBE", etc).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The index of the first line of this block, in the original text's
        /// line count.
        /// </summary>
        public int StartLine { get; }

        /// <summary>
        /// The index of the first line after this block, in the original text's
        /// line count. This will not change if new parameters are added to the
        /// block.
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// The raw lines of the block, including comments and whitespace.
        /// </summary>
        public List<string> RawLines { get; } = new();

        /// <summary>
        /// The parameters in the block, mapped by name.
        /// </summary>
        public Dictionary<string, InstructionParameter> Parameters { get; } = new();

        /// <summary>
        /// The occurrences of parameters in the block, mapped by name.
        /// </summary>
        public List<ParameterOccurrence> ParameterOccurrences { get; } = new();

        /// <summary>
        /// Creates a new block.
        /// </summary>
        /// <param name="type">The type of the block (e.g., "group", "pft").</param>
        /// <param name="name">The name of the block.</param>
        /// <param name="startLine">The index of the first line of this block, in the original text's line count.</param>
        public Block(string type, string name, int startLine)
        {
            Type = type;
            Name = name;
            StartLine = startLine;
        }

        /// <inheritdoc />
        public string ToInsFileString(string lineEnding)
        {
            StringBuilder sb = new();
            foreach (string line in RawLines)
            {
                sb.Append(line);
                sb.Append(lineEnding);
            }
            return sb.ToString();
        }
    }
}
