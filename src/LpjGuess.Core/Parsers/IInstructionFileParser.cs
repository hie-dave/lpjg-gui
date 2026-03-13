using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Interface for parsing instruction files.
/// </summary>
public interface IInstructionFileParser
{
    /// <summary>
    /// Path to the instruction file.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Get the names of all blocks with the specified name.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <returns>The names of all blocks with the specified type.</returns>
    IEnumerable<string> GetBlockNames(string blockType);

    /// <summary>
    /// Gets the value of a parameter within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    InstructionParameter? GetBlockParameter(string blockType, string blockName, string paramName);

    /// <summary>
    /// Gets the raw string value of a parameter within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    string? GetBlockParameterValue(string blockType, string blockName, string paramName);

    /// <summary>
    /// Gets the top-level parameter with the specified name.
    /// </summary>
    /// <param name="name">Name of the parameter</param>
    /// <returns>The parameter if found, null otherwise.</returns>
    InstructionParameter? GetTopLevelParameter(string name);

    /// <summary>
    /// Gets the raw string value of a top-level parameter.
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <returns>Parameter value if found, null otherwise</returns>
    string? GetTopLevelParameterValue(string paramName);

    /// <summary>
    /// Sets a parameter value within a specific block.
    /// </summary>
    /// <param name="blockType">Type of the block (e.g., "group", "pft")</param>
    /// <param name="blockName">Name of the block</param>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">New value</param>
    /// <returns>True if parameter was found and updated, false otherwise</returns>
    void SetBlockParameterValue(string blockType, string blockName, string paramName, string value);

    /// <summary>
    /// Sets a top-level parameter value.
    /// </summary>
    /// <param name="paramName">Name of the parameter</param>
    /// <param name="value">New value</param>
    /// <returns>True if parameter was found and updated, false otherwise</returns>
    bool SetTopLevelParameterValue(string paramName, string value);

    /// <summary>
    /// Enable the PFT with the specified name. Throws if it's not defined.
    /// </summary>
    /// <param name="pft">Name of the PFT to be enabled.</param>
    void EnablePft(string pft);

    /// <summary>
    /// Disable all PFTs defined in the instruction file.
    /// </summary>
    void DisableAllPfts();

    /// <summary>
    /// Check whether a PFT is defined with the given name.
    /// </summary>
    /// <param name="name">Name of the PFT.</param>
    /// <returns>True iff the PFT is defined.</returns>
    bool IsPft(string name);

    /// <summary>
    /// Generates the updated file content with any parameter modifications.
    /// </summary>
    /// <returns>The complete file content as a string</returns>
    string GenerateContent();
}
