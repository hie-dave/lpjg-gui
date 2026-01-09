using System.Security.Cryptography;
using System.Text;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Extensions;

namespace LpjGuess.Runner.Services;

/// <summary>
/// A naming strategy which uses the SHA-256 hash of the simulation as the name.
/// </summary>
public class Sha256NamingStrategy : ISimulationNamingStrategy
{
    /// <summary>
    /// The maximum length of the name.
    /// </summary>
    public int MaxLength { get; set; } = 16;

    /// <inheritdoc/>
    public string GenerateName(ISimulation simulation)
    {
        // The simulation name is assumed to be stable across runs (ie the same
        // set of factors should produce the same name).
        string recipe = simulation.Name;

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(recipe));
        return hash.ToHex()[..MaxLength];
    }
}
