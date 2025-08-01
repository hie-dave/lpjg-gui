using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Services;

namespace LpjGuess.Frontend.Services;

/// <inheritdoc/>
public class WorkspacePathResolver : IPathResolver
{
    /// <summary>
    /// The workspace-level output directory.
    /// </summary>
    private string? outputDirectory;

    /// <summary>
    /// Initialise the resolver with the output directory.
    /// </summary>
    /// <param name="outputDirectory">The output directory.</param>
    public void Initialise(string outputDirectory)
    {
        this.outputDirectory = outputDirectory;
    }

    /// <inheritdoc/>
    public string GenerateTargetInsFilePath(string insFile, ISimulation simulation)
    {
        if (outputDirectory == null)
            throw new InvalidOperationException("Output directory has not been set.");

		string insName = Path.GetFileNameWithoutExtension(insFile);
		string jobName = $"{insName}-{simulation.Name}";

        // TODO: insert experiment path?
		string jobDirectory = Path.Combine(outputDirectory, insName, simulation.Name);

		string targetInsFile = Path.Combine(jobDirectory, $"{jobName}.ins");
		return targetInsFile;
    }
}
