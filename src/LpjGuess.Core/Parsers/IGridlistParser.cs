using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Interface to a parser for gridlist files.
/// </summary>
public interface IGridlistParser
{
    /// <summary>
    /// Parse the specified gridlist file, returning a collection of gridcells.
    /// </summary>
    /// <param name="gridlist">The path to the gridlist file.</param>
    /// <returns>A collection of gridcells parsed from the gridlist file.</returns>
    Task<IEnumerable<Gridcell>> ParseAsync(string gridlist);
}
