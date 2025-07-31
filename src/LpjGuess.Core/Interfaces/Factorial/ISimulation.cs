namespace LpjGuess.Core.Interfaces.Factorial;

/// <summary>
/// Interface to a discrete simulation which can be generated from a base
/// instruction file.
/// </summary>
public interface ISimulation
{
    /// <summary>
    /// Name of this simulation.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Generate this simulation from the specified instruction file.
    /// </summary>
    /// <param name="insFile">Path to a base instruction file.</param>
    /// <param name="targetFile">Path to the instruction file to be generated.</param>
    /// <param name="pfts">List of PFTs to enable. All others will be disabled.</param>
    void Generate(string insFile, string targetFile, IEnumerable<string> pfts);
}
