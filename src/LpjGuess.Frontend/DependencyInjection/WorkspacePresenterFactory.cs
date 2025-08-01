using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace LpjGuess.Frontend.DependencyInjection;

/// <summary>
/// Factory for creating presenters for the workspace.
/// </summary>
public class WorkspacePresenterFactory : PresenterFactory, IDisposable
{
    private readonly IServiceScope scope;

    /// <summary>
    /// Create a new <see cref="WorkspacePresenterFactory"/> instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public WorkspacePresenterFactory(IServiceProvider serviceProvider) : this(serviceProvider.CreateScope())
    {
    }

    /// <summary>
    /// Create a new <see cref="WorkspacePresenterFactory"/> instance.
    /// </summary>
    /// <param name="scope">The service scope.</param>
    private WorkspacePresenterFactory(IServiceScope scope) : base(scope.ServiceProvider)
    {
        this.scope = scope;
    }

    /// <summary>
    /// Initialise the workspace scope with the given instruction files.
    /// </summary>
    /// <param name="workspace"></param>
    public InstructionFilesProvider Initialise(Workspace workspace)
    {
        // Initialise workspace-level data providers.
        var experimentProvider = (ExperimentProvider)scope.ServiceProvider.GetRequiredService<IExperimentProvider>();
        experimentProvider.UpdateExperiments(workspace.Experiments);

        var provider = (InstructionFilesProvider)scope.ServiceProvider.GetRequiredService<IInstructionFilesProvider>();
        provider.UpdateInstructionFiles(workspace.InstructionFiles);
        return provider;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        scope.Dispose();
    }
}
