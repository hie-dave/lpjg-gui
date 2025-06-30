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
    /// <param name="instructionFiles"></param>
    public InstructionFilesProvider Initialise(IEnumerable<string> instructionFiles)
    {
        return ActivatorUtilities.CreateInstance<InstructionFilesProvider>(scope.ServiceProvider, instructionFiles);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        scope.Dispose();
    }
}
