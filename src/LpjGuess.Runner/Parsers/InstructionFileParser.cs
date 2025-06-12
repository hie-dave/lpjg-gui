using System.Text;
using LpjGuess.Runner.Extensions;
using LpjGuess.Runner.Models;
using Tomlyn.Syntax;

namespace LpjGuess.Runner.Parsers;

/// <summary>
/// Parser for LPJ-GUESS instruction files that handles nested blocks, parameters, and comments.
/// </summary>
public class InstructionFileParser
{
    private const string blockPft = "pft";
    private const string includeParameter = "include";

    private class ParameterOccurrence
    {
        public string Name { get; set; } = "";
        public InstructionParameter Value { get; set; } = new("");
        public int LineNumber { get; set; }
        public string OriginalLine { get; set; } = "";
    }

    private class Block
    {
        public string Type { get; }
        public string Name { get; }
        public int StartLine { get; }
        /// <summary>
        /// The index of the first line after this block, in the original text's
        /// line count. This will not change if new parameters are added to the
        /// block.
        /// </summary>
        public int EndLine { get; set; }
        public List<string> RawLines { get; } = new();
        public Dictionary<string, InstructionParameter> Parameters { get; } = new();
        public List<ParameterOccurrence> ParameterOccurrences { get; } = new();

        public Block(string type, string name, int startLine)
        {
            Type = type;
            Name = name;
            StartLine = startLine;
        }
    }

    private readonly List<Block> _blocks;
    private readonly Dictionary<string, InstructionParameter> _topLevelParams;
    private List<string> _lines;
    private string _rawContent = string.Empty;
    private int _currentLine;
    private string _lineEnding = "\n"; // Default

    /// <summary>
    /// Path to the instruction file.
    /// </summary>
    public string FilePath { get; private init; }

    public InstructionFileParser(string content, string path)
    {
        FilePath = path;
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        _rawContent = content;
        _lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        _blocks = new List<Block>();
        _topLevelParams = new Dictionary<string, InstructionParameter>();
        _currentLine = 0;

        // Ensure we preserve the exact line endings from original content
        if (content.EndsWith("\r\n"))
            _lineEnding = "\r\n";
        else if (content.EndsWith("\n"))
            _lineEnding = "\n";
        else
            _lineEnding = "\n"; // Default
        
        Parse();
    }

    public static InstructionFileParser FromFile(string path)
    {
        string contents = InstructionFileNormaliser.Normalise(path);
        return new InstructionFileParser(contents, path);
    }

    private void Parse()
    {
        // First, normalize line endings to \n
        _lines = _lines.Select(line => line.TrimEnd('\r')).ToList();
        _rawContent = string.Join("\n", _lines);

        for (_currentLine = 0; _currentLine < _lines.Count; _currentLine++)
        {
            string currentLine = _lines[_currentLine];
            string trimmedCurrentLine = currentLine.Trim();

            if (IsBlockStart(trimmedCurrentLine, out string blockType, out string blockName))
            {
                var block = new Block(blockType, blockName, _currentLine);
                var parenthesesCount = 1;
                
                block.RawLines.Add(currentLine);

                while (++_currentLine < _lines.Count && parenthesesCount > 0)
                {
                    string blockLine = _lines[_currentLine];
                    block.RawLines.Add(blockLine);

                    // Skip comments
                    if (blockLine.Trim().StartsWith('!'))
                        continue;

                    // Count parentheses, but ignore those in comments
                    var commentIndex = blockLine.IndexOf('!');
                    var lineToCheck = commentIndex >= 0 ? blockLine[..commentIndex] : blockLine;
                    
                    parenthesesCount += lineToCheck.Count(c => c == '(');
                    parenthesesCount -= lineToCheck.Count(c => c == ')');

                    // If we're still in the block, try to parse parameters
                    if (parenthesesCount > 0)
                    {
                        string trimmedLine = blockLine.Trim();
                        if (TryParseParameter(trimmedLine, out string paramName, out string paramValue))
                        {
                            var param = new InstructionParameter(paramValue);
                            block.Parameters[paramName] = param;
                            
                            block.ParameterOccurrences.Add(new ParameterOccurrence
                            {
                                Name = paramName,
                                Value = param,
                                LineNumber = _currentLine - block.StartLine,
                                OriginalLine = blockLine
                            });
                        }
                    }
                }

                block.EndLine = _currentLine;
                _blocks.Add(block);
                
                // Move back one line since the main loop will increment
                _currentLine--;
            }
            else if (!string.IsNullOrWhiteSpace(trimmedCurrentLine) && !trimmedCurrentLine.StartsWith('!'))
            {
                // Handle top-level parameters
                if (TryParseParameter(trimmedCurrentLine, out string paramName, out string paramValue))
                {
                    _topLevelParams[paramName] = new InstructionParameter(paramValue);
                }
            }
        }
    }

    private bool IsBlockStart(string line, out string blockType, out string blockName)
    {
        blockType = string.Empty;
        blockName = string.Empty;

        // Replace everything after a comment character, if one is present.
        line = line.SplitHonouringQuotes(['!']).First();

        // Match patterns like: group "C3G" ( or pft "TeBE" (
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 && parts[^1] == "(" && parts[1].StartsWith('"') && parts[1].EndsWith('"'))
        {
            blockType = parts[0];
            blockName = parts[1].Trim('"');
            return true;
        }

        return false;
    }


    private class ParameterInfo
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public string ValueSpacing { get; set; } = " "; // Space between name and value
        public string CommentSpacing { get; set; } = "    "; // Space between value and comment
    }

    private bool TryParseParameter(string line, out string name, out string value)
    {
        name = string.Empty;
        value = string.Empty;

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('!'))
            return false;

        var info = ParseParameterLine(line);
        if (info != null)
        {
            name = info.Name;
            value = info.Value;
            return true;
        }

        return false;
    }

    private ParameterInfo? ParseParameterLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        // Check if this is a comment-only line
        int commentStart = line.IndexOf('!');
        if (commentStart == line.TrimStart().Length - line.Length)
            return null;

        // Find the first non-whitespace character
        int nameStart = 0;
        while (nameStart < line.Length && char.IsWhiteSpace(line[nameStart]))
            nameStart++;

        // Skip if this is a comment-only line
        if (nameStart >= line.Length || line[nameStart] == '!')
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
            else if (line[valueEnd] == '!' && !inQuotes)
                break;
            valueEnd++;
        }

        // Trim trailing whitespace from value
        while (valueEnd > valueStart && char.IsWhiteSpace(line[valueEnd - 1]))
            valueEnd--;

        if (valueEnd <= valueStart) // No value found
            return null;

        return new ParameterInfo
        {
            Name = name,
            Value = line[valueStart..valueEnd],
            ValueSpacing = line[nameEnd..valueStart],
            CommentSpacing = valueEnd < line.Length ? line[valueEnd..(line.IndexOf('!', valueEnd))] : ""
        };
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
        var block = _blocks.FirstOrDefault(b => b.Type == blockType && b.Name == blockName);
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
    /// Gets the value of a top-level parameter.
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    public InstructionParameter? GetTopLevelParameter(string paramName)
    {
        return _topLevelParams.GetValueOrDefault(paramName);
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
	/// Change a parameter in the instruction file.
	/// </summary>
	/// <param name="factor">The parameter name and new value.</param>
	public void ApplyChange(Factor factor)
	{
		const char sep = '.';
		if (!factor.Name.Contains(sep))
			// SetParameterValue(factor.Name, factor.Value);
			SetTopLevelParameterValue(factor.Name, factor.Value);
		else
		{
			string[] parts = factor.Name.Split(sep);
			if (parts.Length != 2)
				throw new ArgumentException($"If a parameter name contains a period, it must be of the form: block.param. E.g. TeBE.sla. Parameter name: {factor.Name}");
			string blockName = parts[0];
			string paramName = parts[1];
			string? blockType = _blocks.FirstOrDefault(b => b.Name == blockName)?.Type;
			if (blockType == null)
				throw new ArgumentException($"No block found with name: {blockName} for parameter '{factor.Name}'");
			SetBlockParameterValue(blockType, blockName, paramName, factor.Value);
		}
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
            var newLine = $"{indent}{paramName} {newParam.AsString()}";
            
            // Add to RawLines just before closing parenthesis
            int insertPosition = block.RawLines.Count - 1;
            block.RawLines.Insert(insertPosition, newLine);

            // Add new occurrence
            block.ParameterOccurrences.Add(new ParameterOccurrence
            {
                Name = paramName,
                Value = newParam,
                LineNumber = insertPosition - 1, // -1 to account for block header
                OriginalLine = newLine
            });
        }
        else
        {
            // For existing parameters, update all occurrences
            ParameterOccurrence occurrence = block.ParameterOccurrences.Last(p => p.Name == paramName);
            occurrence.Value = newParam;

            // Update the line in RawLines
            string indent = occurrence.OriginalLine[..occurrence.OriginalLine.IndexOf(paramName)];
            string newLine = $"{indent}{paramName} {newParam.AsString()}";

            // Preserve any comment after the parameter
            int commentIndex = occurrence.OriginalLine.IndexOf('!');
            if (commentIndex >= 0)
            {
                newLine += occurrence.OriginalLine[commentIndex..];
            }

            // Update the line in RawLines
            block.RawLines[occurrence.LineNumber] = newLine;
        }
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
        var block = _blocks.FirstOrDefault(b => b.Type == blockType && b.Name == blockName);
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
        if (!_topLevelParams.ContainsKey(paramName))
            return false;

        _topLevelParams[paramName] = new InstructionParameter(value);
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
        foreach (var block in _blocks.Where(b => b.Type == blockPft))
            SetBlockParameterValue(block, includeParameter, "0");
    }

    /// <summary>
    /// Check whether a PFT is defined with the given name.
    /// </summary>
    /// <param name="name">Name of the PFT.</param>
    /// <returns>True iff the PFT is defined.</returns>
    public bool IsPft(string name)
    {
        return _blocks.Any(b => b.Type == blockPft && b.Name == name);
    }

    /// <summary>
    /// Generates the updated file content with any parameter modifications.
    /// </summary>
    /// <returns>The complete file content as a string</returns>
    public string GenerateContent()
    {
        if (_lines == null || _lines.Count == 0)
            return string.Empty;

        var content = new StringBuilder();
        
        // Handle case where there are no blocks (just top-level content)
        if (_blocks.Count == 0)
            return string.Join(_lineEnding, _lines) + (_rawContent.EndsWith(_lineEnding) ? _lineEnding : "");

        var currentLine = 0;
        
        foreach (var block in _blocks.OrderBy(b => b.StartLine))
        {
            // Add content before block exactly as is
            while (currentLine < block.StartLine)
            {
                content.Append(_lines[currentLine]);
                
                // Add line ending unless it's the last line and original didn't end with one
                if (currentLine < _lines.Count - 1 || _rawContent.EndsWith(_lineEnding))
                    content.Append(_lineEnding);
                
                currentLine++;
            }

            // Add block content exactly as parsed
            for (int i = 0; i < block.RawLines.Count; i++)
            {
                content.Append(block.RawLines[i]);
                
                // Add line ending.
                // Note that we split on newline, so the implication is that
                // every line in the block is a discrete line and must therefore
                // end with a line ending. The only exception is if this is the
                // last block in the file, and the file doesn't end with a
                // newline character. That's actually not supported by lpj-guess
                // (the model will crash), but we handle it here anyway, just
                // for fun.
                if (i == block.RawLines.Count - 1 && // Last line in block
                block.StartLine == _blocks.Max(b => b.StartLine) && // Is last block
                block.EndLine == _lines.Count && // Block ends at end of document
                !_rawContent.EndsWith(_lineEnding)) // Document doesn't end with newline
                    continue;
                content.Append(_lineEnding);
            }
        
            // Move to first line after block
            currentLine = block.EndLine;
        
            // Preserve all whitespace exactly as in original until next block starts
            while (currentLine < _lines.Count && 
                   (string.IsNullOrWhiteSpace(_lines[currentLine]) || 
                    !_blocks.Any(b => b.StartLine == currentLine)))
            {
                content.Append(_lines[currentLine]);
                
                // Add line ending unless it's the last line
                if (currentLine < _lines.Count - 1)
                    content.Append(_lineEnding);
                
                currentLine++;
            }
        }

        // Add remaining content exactly as is
        while (currentLine < _lines.Count)
        {
            content.Append(_lines[currentLine]);
            
            // Add line ending unless it's the last line and original didn't end with one
            if (currentLine < _lines.Count - 1 || _rawContent.EndsWith(_lineEnding))
                content.Append(_lineEnding);
            
            currentLine++;
        }

        return content.ToString();
    }
}
