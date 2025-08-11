using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Runner.Services;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Encapsulates the instruction files in a workspace.
/// </summary>
public class InstructionFilesProvider : IInstructionFilesProvider
{
    /// <summary>
    /// The experiment provider.
    /// </summary>
    private readonly IExperimentProvider experimentsProvider;

    /// <summary>
    /// The path resolver.
    /// </summary>
    private readonly IPathResolver resolver;

    /// <summary>
    /// The instruction files in the workspace.
    /// </summary>
    private List<string> instructionFiles;

    /// <summary>
    /// Event raised when the instruction files change.
    /// </summary>
    public Event<IEnumerable<string>> OnInstructionFilesChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="InstructionFilesProvider"/> instance.
    /// </summary>
    public InstructionFilesProvider(IExperimentProvider provider, IPathResolver resolver)
    {
        experimentsProvider = provider;
        this.resolver = resolver;
        OnInstructionFilesChanged = new Event<IEnumerable<string>>();
        instructionFiles = [];
    }

    /// <summary>
    /// Get the instruction files in the workspace.
    /// </summary>
    /// <returns>The instruction files.</returns>
    public IEnumerable<string> GetInstructionFiles() => instructionFiles;

    /// <summary>
    /// Update the instruction files.
    /// </summary>
    /// <param name="instructionFiles">The instruction files.</param>
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles.ToList();
        OnInstructionFilesChanged.Invoke(instructionFiles);
    }

    /// <inheritdoc />
    public IEnumerable<InstructionFile> GetGeneratedInstructionFiles()
    {
        List<InstructionFile> concreteInsFiles = [];
        foreach (Experiment experiment in experimentsProvider.GetExperiments())
        {
            foreach (ISimulation simulation in experiment.SimulationGenerator.Generate())
            {
                foreach (string insFile in instructionFiles)
                {
                    if (experiment.DisabledInsFiles.Contains(insFile))
                        continue;
                    string generated = resolver.GenerateTargetInsFilePath(insFile, simulation);
                    concreteInsFiles.Add(new InstructionFile(generated, experiment.Name, simulation.Name));
                }
            }
        }
        return concreteInsFiles;
    }
}
