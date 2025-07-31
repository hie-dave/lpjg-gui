using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Parser for LPJ-GUESS instruction files that handles nested blocks, parameters, and comments.
/// </summary>
public partial class InstructionFileParser
{
    /// <summary>
    /// The character used to mark a comment in an instruction file.
    /// </summary>
    private const char commentChar = '!';

    /// <summary>
    /// Type of blocks used to designate PFT blocks.
    /// </summary>
    private const string blockPft = "pft";

    /// <summary>
    /// Name of the parameter used to define whether a particular PFT is
    /// included in a simulation.
    /// </summary>
    private const string includeParameter = "include";

    /// <summary>
    /// A regular expression for detecting the start of a block.
    /// </summary>
    [GeneratedRegex(@"^\s*(\w+)\s+\""([^\""]+)\""\s*\((.*)")]
    private static partial Regex BlockStartRegex();

    /// <summary>
    /// The parsed items in the file.
    /// </summary>
    private readonly List<IFileContent> items;

    /// <summary>
    /// The line endings used in the original file.
    /// </summary>
    private readonly string lineEnding;

    /// <summary>
    /// Path to the instruction file.
    /// </summary>
    public string FilePath { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFileParser"/> instance. This will not
    /// resolve the import directives into the file. If that is required, use
    /// <see cref="FromFile(string)"/>.
    /// </summary>
    /// <param name="content">The content of the instruction file.</param>
    /// <param name="path">The path to the instruction file.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="content"/> is null or whitespace.</exception>
    public InstructionFileParser(string content, string path)
    {
        FilePath = path;
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        // Ensure we preserve the exact line endings from original content
        lineEnding = GuessLineEnding(content);

        try
        {
            items = Parse(content);
        }
        catch (Exception error)
        {
            throw new Exception($"Unable to parse instruction file '{path}'", error);
        }
    }

    /// <summary>
    /// Create a new <see cref="InstructionFileParser"/> instance from a file,
    /// recursively resolving all import directives in the file.
    /// </summary>
    /// <param name="path">The path to the instruction file.</param>
    /// <returns>A new <see cref="InstructionFileParser"/> instance.</returns>
    public static InstructionFileParser FromFile(string path)
    {
        string contents = InstructionFileNormaliser.Normalise(path);
        return new InstructionFileParser(contents, path);
    }

    /// <summary>
    /// Gets the value of a parameter within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    public InstructionParameter? GetBlockParameter(string blockType, string blockName, string paramName)
    {
        Block? block = GetBlock(blockType, blockName);
        return block?.Parameters.GetValueOrDefault(paramName);
    }

    /// <summary>
    /// Gets the raw string value of a parameter within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    public string? GetBlockParameterValue(string blockType, string blockName, string paramName)
    {
        return GetBlockParameter(blockType, blockName, paramName)?.AsString();
    }

    /// <summary>
    /// Gets the top-level parameter with the specified name.
    /// </summary>
    /// <param name="name">Name of the parameter</param>
    /// <returns>The parameter if found, null otherwise.</returns>
    public InstructionParameter? GetTopLevelParameter(string name)
    {
        return GetTopLevelParameterOccurrence(name)?.Value;
    }

    /// <summary>
    /// Gets the raw string value of a top-level parameter.
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    public string? GetTopLevelParameterValue(string paramName)
    {
        return GetTopLevelParameter(paramName)?.AsString();
    }

    /// <summary>
    /// Sets a parameter value within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">New value</param>
    /// <returns>True if parameter was found and updated, false otherwise</returns>
    public void SetBlockParameterValue(string blockType, string blockName, string paramName, string value)
    {
        Block? block = GetBlock(blockType, blockName);
        if (block == null)
            throw new InvalidOperationException($"No block found named '{blockName}' with type '{blockType}'");

        SetBlockParameterValue(block, paramName, value);
    }

    /// <summary>
    /// Sets a top-level parameter value.
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">New value</param>
    /// <returns>True if parameter was found and updated, false otherwise</returns>
    public bool SetTopLevelParameterValue(string paramName, string value)
    {
        // We could add a new top-level parameter if it doesn't exist. For now,
        // let's just fail, under the assumption that this is a user error.
        ParameterOccurrence? parameter = GetTopLevelParameterOccurrence(paramName);
        if (parameter is null)
            return false;

        // Update parameter value.
        parameter.Value = InstructionParameter.FromUserInput(value);

        return true;
    }

    /// <summary>
    /// Enable the PFT with the specified name. Throws if it's not defined.
    /// </summary>
    /// <param name="pft">Name of the PFT to be enabled.</param>
    public void EnablePft(string pft)
    {
        SetBlockParameterValue(blockPft, pft, includeParameter, "1");
    }

    /// <summary>
    /// Disable all PFTs defined in the instruction file.
    /// </summary>
    public void DisableAllPfts()
    {
        foreach (Block block in GetPfts())
            SetBlockParameterValue(block, includeParameter, "0");
    }

    /// <summary>
    /// Check whether a PFT is defined with the given name.
    /// </summary>
    /// <param name="name">Name of the PFT.</param>
    /// <returns>True iff the PFT is defined.</returns>
    public bool IsPft(string name)
    {
        return GetPfts().Any(b => b.Name == name);
    }

    /// <summary>
    /// Generates the updated file content with any parameter modifications.
    /// </summary>
    /// <returns>The complete file content as a string</returns>
    public string GenerateContent()
    {
        if (items.Count == 0)
            return string.Empty;

        var content = new StringBuilder();

        foreach (IFileContent item in items)
            content.Append(item.ToInsFileString(lineEnding));

        // Each item will insert a line ending after itself. Therefore, the
        // document will end with one extra line ending that the original did
        // not have. We need to remove this.
        int nchar = lineEnding.Length;
        content.Remove(content.Length - nchar, nchar);

        return content.ToString();
    }

    /// <summary>
    /// Guess the line ending used in the file.
    /// </summary>
    /// <param name="content">The content of the file.</param>
    /// <returns>The line ending used in the file.</returns>
    private static string GuessLineEnding(string content)
    {
        if (content.EndsWith("\r\n") || content.Contains("\r\n"))
            return "\r\n";
        else if (content.EndsWith("\n"))
            return "\n";
        else
            // Default
            return "\n";
    }

    /// <summary>
    /// Parse the instruction file content.
    /// </summary>
    /// <param name="content">The content to parse.</param>
    private List<IFileContent> Parse(string content)
    {
        // First, normalize line endings to \n
        List<string> lines = content
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToList();
        content = string.Join("\n", lines);

        List<IFileContent> items = [];
        for (int lineNumber = 0; lineNumber < lines.Count; lineNumber++)
        {
            try
            {
                items.Add(ParseLine(lines, ref lineNumber));
            }
            catch (Exception error)
            {
                throw new Exception($"Parser error on line {lineNumber + 1}", error);
            }
        }

        return items;
    }

    /// <summary>
    /// Parse a file content object form the current line in the instruction
    /// file. Some content types (specifically, blocks) can span multiple lines,
    /// so lineNumber may be incremented by the method. After returning though,
    /// the next line that should be parsed will be lineNumber + 1.
    /// </summary>
    /// <param name="lines">The lines of the file.</param>
    /// <param name="lineNumber">The current line number.</param>
    /// <returns>
    /// A <see cref="IFileContent"/> object representing the content of the
    /// line.
    /// </returns>
    private IFileContent ParseLine(List<string> lines, ref int lineNumber)
    {
        string currentLine = lines[lineNumber];
        string trimmedCurrentLine = currentLine.Trim();

        if (string.IsNullOrWhiteSpace(trimmedCurrentLine) || trimmedCurrentLine.StartsWith(commentChar))
            return new VerbatimLine(currentLine, lineNumber);

        if (IsBlockStart(trimmedCurrentLine, out string? blockType, out string? blockName, out string? inlineContent))
            return ParseBlock(lines, currentLine, blockType, blockName, inlineContent, ref lineNumber);
        else
            return ParseTopLevelParameter(currentLine, lineNumber);
    }

    /// <summary>
    /// Parse a block from the instruction file.
    /// </summary>
    /// <param name="lines">The lines of the file.</param>
    /// <param name="currentLine">The current line.</param>
    /// <param name="blockType">The type of the block.</param>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="inlineContent">The inline content of the block.</param>
    /// <param name="lineNumber">Line number at which the block starts. This will be incremented by the method if the block contains multiple lines.</param>
    /// <returns>A <see cref="Block"/> object representing the block.</returns>
    private Block ParseBlock(
        IReadOnlyList<string> lines,
        string currentLine,
        string blockType,
        string blockName,
        string inlineContent,
        ref int lineNumber)
    {
        Block block = new Block(blockType, blockName, lineNumber);
        block.RawLines.Add(currentLine);

        // A list of all lines inside the block's parentheses, starting with any inline content.
        List<string> contentLines = [inlineContent];

        // First, check parenthesis balance on the inline content.
        int parenthesesCount = 1;
        string lineToCheck = inlineContent.SplitHonouringQuotes([commentChar]).First();
        parenthesesCount += lineToCheck.Count(c => c == '(');
        parenthesesCount -= lineToCheck.Count(c => c == ')');

        // Gather subsequent lines until the block is closed.
        int currentBlockLine = lineNumber;
        while (++currentBlockLine < lines.Count && parenthesesCount > 0)
        {
            string blockLine = lines[currentBlockLine];
            block.RawLines.Add(blockLine);
            contentLines.Add(blockLine);

            lineToCheck = blockLine.SplitHonouringQuotes([commentChar]).First();
            parenthesesCount += lineToCheck.Count(c => c == '(');
            parenthesesCount -= lineToCheck.Count(c => c == ')');
        }

        // Parse all the parameters from the collected content.
        for (int i = 0; i < contentLines.Count; i++)
        {
            string contentLine = contentLines[i];
            string contentToParse = contentLine.SplitHonouringQuotes([commentChar]).First();

            if (TryParseParameter(contentToParse, out ParameterInfo? info))
            {
                InstructionParameter param = new(info.Value);
                block.Parameters[info.Name] = param;

                block.ParameterOccurrences.Add(new ParameterOccurrence(
                    info.Name,
                    param,
                    i, // Relative line number in block content
                    contentToParse, // The exact substring that was parsed
                    info.PreNameSpacing,
                    info.PreValueSpacing,
                    info.PostValue
                ));
            }
        }

        block.EndLine = currentBlockLine - 1;
        lineNumber = block.EndLine;

        return block;
    }

    /// <summary>
    /// Parse a top-level parameter from the specified line.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <param name="lineNumber">The line number.</param>
    /// <returns>A <see cref="ParameterOccurrence"/> object representing the parameter.</returns>
    /// <exception cref="ArgumentException">Thrown if the line is invalid (e.g. a name without a value).</exception>
    private ParameterOccurrence ParseTopLevelParameter(string line, int lineNumber)
    {
        if (!TryParseParameter(line.Trim(), out ParameterInfo? info))
            throw new ArgumentException($"Invalid line: {line}");

        return new ParameterOccurrence(
            info.Name,
            new InstructionParameter(info.Value),
            lineNumber,
            line,
            info.PreNameSpacing,
            info.PreValueSpacing,
            info.PostValue);
    }

    /// <summary>
    /// Determine if the specified line is the start of a block.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <param name="blockType">The type of the block.</param>
    /// <param name="blockName">The name of the block.</param>
    /// <param name="inlineContent">The inline content of the block.</param>
    /// <returns>True iff the line is the start of a block.</returns>
    private bool IsBlockStart(string line, [NotNullWhen(true)] out string? blockType, [NotNullWhen(true)] out string? blockName, [NotNullWhen(true)] out string? inlineContent)
    {
        // Replace everything after a comment character, if one is present.
        line = line.SplitHonouringQuotes([commentChar]).First();

        Match match = BlockStartRegex().Match(line);
        if (match.Success)
        {
            // Group 1 captures the block type, e.g., "group"
            blockType = match.Groups[1].Value;

            // Group 2 captures the content of the quotes, e.g., "shrub"
            blockName = match.Groups[2].Value;

            // Group 3 captures any content within the parentheses on the same line.
            inlineContent = match.Groups[3].Value;
            return true;
        }

        blockType = null;
        blockName = null;
        inlineContent = null;
        return false;
    }

    /// <summary>
    /// Try to parse a parameter from the specified line.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <param name="metadata">Parameter metadata.</param>
    /// <returns>True if the line is a parameter, false otherwise.</returns>
    private bool TryParseParameter(string line, [NotNullWhen(true)] out ParameterInfo? metadata)
    {
        metadata = null;

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(commentChar))
            return false;

        metadata = ParseParameterLine(line);
        return metadata != null;
    }

    /// <summary>
    /// Parse a parameter from the specified line.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>The parsed parameter, or null if the line is not a parameter.</returns>
    private ParameterInfo? ParseParameterLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        // Check if this is a comment-only line
        int commentStart = line.IndexOf(commentChar);
        if (commentStart == line.TrimStart().Length - line.Length)
            return null;

        // Find the first non-whitespace character
        int nameStart = 0;
        while (nameStart < line.Length && char.IsWhiteSpace(line[nameStart]))
            nameStart++;

        // Skip if this is a comment-only line
        if (nameStart >= line.Length || line[nameStart] == commentChar)
            return null;

        // Find the end of the name (next whitespace)
        int nameEnd = nameStart;
        while (nameEnd < line.Length && !char.IsWhiteSpace(line[nameEnd]))
            nameEnd++;

        if (nameEnd >= line.Length) // No value found
            return null;

        string name = line[nameStart..nameEnd];

        // Find the start of the value
        int valueStart = nameEnd;
        while (valueStart < line.Length && char.IsWhiteSpace(line[valueStart]))
            valueStart++;

        // Find the end of the value (at comment or end of line)
        int valueEnd = valueStart;
        bool inQuotes = false;
        while (valueEnd < line.Length)
        {
            if (line[valueEnd] == '"')
                inQuotes = !inQuotes;
            else if (!inQuotes && (line[valueEnd] == commentChar || line[valueEnd] == ')'))
                break;
            valueEnd++;
        }

        // Trim trailing whitespace from value
        while (valueEnd > valueStart && char.IsWhiteSpace(line[valueEnd - 1]))
            valueEnd--;

        if (valueEnd <= valueStart) // No value found
            return null;

        string value = line[valueStart..valueEnd];
        string preNameSpacing = line[..nameStart];
        string preValueSpacing = line[nameEnd..valueStart];
        string postValue = line[valueEnd..line.Length];
        return new ParameterInfo(name, value, preNameSpacing, preValueSpacing, postValue);
    }

    private void SetBlockParameterValue(Block block, string paramName, string value)
    {
        InstructionParameter newParam = new InstructionParameter(value);
        bool isNewParameter = !block.Parameters.ContainsKey(paramName);

        // Update the parameter dictionary
        block.Parameters[paramName] = newParam;

        if (isNewParameter)
        {
            // For new parameters, add them at the end of the block
            string indent = "    "; // Default indentation
            if (block.ParameterOccurrences.Count > 0)
            {
                indent = block.ParameterOccurrences[0].OriginalLine[..block.ParameterOccurrences[0].OriginalLine.IndexOf(block.ParameterOccurrences[0].Name)];
            }

            // Create new parameter line
            var newLine = $"{indent}{paramName} {newParam.ToInsFileString()}";

            // Add to RawLines just before closing parenthesis
            int insertPosition = block.RawLines.Count - 1;
            block.RawLines.Insert(insertPosition, newLine);

            // Add new occurrence
            block.ParameterOccurrences.Add(new ParameterOccurrence(
                paramName,
                newParam,
                insertPosition - 1, // -1 to account for block header
                newLine,
                "",  // No spacing before the name
                " ", // One whitespace between the name and value
                ""   // No spacing after the value
            ));
        }
        else
        {
            // For existing parameters, update the last occurrence.
            ParameterOccurrence occurrence = block.ParameterOccurrences.Last(p => p.Name == paramName);
            string oldParameterLine = occurrence.OriginalLine;

            // Create the new parameter line string.
            occurrence.Value = new InstructionParameter(value);
            string newValueString = occurrence.Value.ToInsFileString();
            string newParameterLine = $"{occurrence.PreNameSpacing}{occurrence.Name}{occurrence.PreValueSpacing}{newValueString}{occurrence.PostValue}";

            // Find the raw line containing the old parameter text and replace it.
            // This is more robust than relying on indices, especially for inline blocks.
            for (int i = 0; i < block.RawLines.Count; i++)
            {
                if (block.RawLines[i].Contains(oldParameterLine))
                {
                    block.RawLines[i] = block.RawLines[i].Replace(oldParameterLine, newParameterLine);
                    break; // Assume the first match is the correct one.
                }
            }
        }
    }

    /// <summary>
    /// Get all PFT blocks.
    /// </summary>
    /// <returns>All blocks of type PFT.</returns>
    private IEnumerable<Block> GetPfts()
    {
        return items.OfType<Block>().Where(b => b.Type == blockPft);
    }

    /// <summary>
    /// Get the block with the specified name and type.
    /// </summary>
    /// <param name="type">Type of the block (e.g. "group", "pft").</param>
    /// <param name="name">Name of the block (e.g. "tree" or "TeBE").</param>
    /// <returns>The block if found, null otherwise</returns>
    private Block? GetBlock(string type, string name)
    {
        return items
            .OfType<Block>()
            .FirstOrDefault(b => b.Type == type && b.Name == name);
    }

    /// <summary>
    /// Get the top-level parameter with the specified name.
    /// </summary>
    /// <param name="name">Name of the top-level parameter (e.g. "wateruptake").</param>
    /// <returns>The parameter if found, null otherwise.</returns>
    private ParameterOccurrence? GetTopLevelParameterOccurrence(string name)
    {
        return items
            .OfType<ParameterOccurrence>()
            .FirstOrDefault(p => p.Name == name);
    }
}
