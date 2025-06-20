namespace LpjGuess.Runner.Parsers;

public partial class InstructionFileParser
{
    /// <summary>
    /// Information about a parameter, including its name, value, and spacing.
    /// </summary>
    private class ParameterInfo
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whitespace before the parameter's name.
        /// </summary>
        public string PreNameSpacing { get; set; }

        /// <summary>
        /// Space between the parameter's name and value.
        /// </summary>
        public string PreValueSpacing { get; set; }

        /// <summary>
        /// Space between the parameter's value and any comment.
        /// </summary>
        public string PostValue { get; set; }

        /// <summary>
        /// Create a new <see cref="ParameterInfo"/> instance.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="preNameSpacing">Whitespace before the parameter's name.</param>
        /// <param name="preValueSpacing">Space between the parameter name and value.</param>
        /// <param name="postValue">Everything in the line after the parameter value.</param>
        public ParameterInfo(string name, string value, string preNameSpacing, string preValueSpacing, string postValue)
        {
            Name = name;
            Value = value;
            PreNameSpacing = preNameSpacing;
            PreValueSpacing = preValueSpacing;
            PostValue = postValue;
        }
    }
}
