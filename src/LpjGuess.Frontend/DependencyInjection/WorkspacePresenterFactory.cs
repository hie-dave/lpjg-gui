using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.Services;
using LpjGuess.Runner.Services;
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
    /// <param name="workspace">The workspace.</param>
    public InstructionFilesProvider Initialise(Workspace workspace)
    {
        // Initialise workspace-level data providers.
        var experimentProvider = (ExperimentProvider)scope.ServiceProvider.GetRequiredService<IExperimentProvider>();
        experimentProvider.UpdateExperiments(workspace.Experiments);

        IWorkspacePathHelper pathResolver = scope.ServiceProvider.GetRequiredService<IWorkspacePathHelper>();
        if (pathResolver is not WorkspacePathResolver resolver)
            throw new InvalidOperationException("Path resolver is not a workspace path resolver.");
        resolver.Initialise(workspace.GetOutputDirectory());

        var provider = (InstructionFilesProvider)scope.ServiceProvider.GetRequiredService<IInstructionFilesProvider>();
        provider.UpdateInstructionFiles(workspace.InstructionFiles);
        return provider;
    }

    /// <summary>
    /// Get the path resolver.
    /// </summary>
    public IWorkspacePathHelper GetPathHelper()
    {
        return scope.ServiceProvider.GetRequiredService<IWorkspacePathHelper>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        scope.Dispose();
    }
}
