using System.Text;
using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

public partial class InstructionFileParser
{
    /// <summary>
    /// Information about a parameter, including its name, value, and spacing.
    /// </summary>
    private class ParameterOccurrence : IFileContent
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; private init; }

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public InstructionParameter Value { get; set; }

        /// <summary>
        /// The line number at which the parameter occurs. Within a block, this
        /// is relative to the start line of the block.
        /// </summary>
        public int LineNumber { get; private init; }

        /// <summary>
        /// The original line of the parameter.
        /// </summary>
        public string OriginalLine { get; set; }

        /// <summary>
        /// The start index of this parameter fragment within the raw line.
        /// </summary>
        public int StartIndex { get; private init; }

        /// <summary>
        /// The length of this parameter fragment within the raw line.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The whitespace before the parameter name.
        /// </summary>
        public string PreNameSpacing { get; private init; }

        /// <summary>
        /// The spacing between the parameter name and value.
        /// </summary>
        public string PreValueSpacing { get; private init; }

        /// <summary>
        /// Everything that appears on the parameter line after the value.
        /// </summary>
        public string PostValue { get; private init; }

        /// <summary>
        /// Create a new <see cref="ParameterOccurrence"/> instance.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="lineNumber">The line number at which the parameter occurs.</param>
        /// <param name="originalLine">The original line of the parameter.</param>
        /// <param name="startIndex">The start index of this parameter fragment within the raw line.</param>
        /// <param name="length">The length of this parameter fragment within the raw line.</param>
        /// <param name="preNameSpacing">The spacing before the parameter name.</param>
        /// <param name="preValueSpacing">The spacing between the parameter name and value.</param>
        /// <param name="postValue">Everything that appears on the parameter line after the value.</param>
        public ParameterOccurrence(
            string name,
            InstructionParameter value,
            int lineNumber,
            string originalLine,
            int startIndex,
            int length,
            string preNameSpacing,
            string preValueSpacing,
            string postValue)
        {
            Name = name;
            Value = value;
            LineNumber = lineNumber;
            OriginalLine = originalLine;
            StartIndex = startIndex;
            Length = length;
            PreNameSpacing = preNameSpacing;
            PreValueSpacing = preValueSpacing;
            PostValue = postValue;
        }

        /// <inheritdoc/>
        public string ToInsFileString(string lineEnding)
        {
            return $"{ToInsFileFragmentString()}{lineEnding}";
        }

        /// <summary>
        /// Render this parameter without a line ending.
        /// </summary>
        public string ToInsFileFragmentString()
        {
            // Update raw line, preserving whitespace and any comments.
            StringBuilder line = new StringBuilder(PreNameSpacing);
            line.Append(Name);
            line.Append(PreValueSpacing);
            line.Append(Value.ToInsFileString());
            line.Append(PostValue);
            return line.ToString();
        }
    }
}
