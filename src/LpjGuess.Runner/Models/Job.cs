using LpjGuess.Core.Models;

namespace LpjGuess.Runner.Models;

/// <summary>
/// A job to be run.
/// </summary>
public class Job
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// The path to the ins file.
    /// </summary>
    public string InsFile { get; private init; }

    /// <summary>
    /// The simulation manifest.
    /// </summary>
    public SimulationManifest Manifest { get; private init; }

    /// <summary>
    /// Create a new <see cref="Job"/> instance.
    /// </summary>
    /// <param name="name">The name of the job.</param>
    /// <param name="insFile">The path to the ins file.</param>
    /// <param name="manifest">The simulation manifest.</param>
    public Job(string name, string insFile, SimulationManifest manifest)
    {
        Name = name;
        InsFile = insFile;
        Manifest = manifest;
    }
}
