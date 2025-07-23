using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a top-level factor generator.
/// </summary>
public class BlockFactorGeneratorPresenter : TopLevelFactorGeneratorPresenter
{
    /// <summary>
    /// The model instance managed by this presenter.
    /// </summary>
    private readonly BlockFactorGenerator block;

    /// <summary>
    /// The view managed by this presenter.
    /// </summary>
    private new readonly IBlockFactorGeneratorView view;

    /// <summary>
    /// Create a new <see cref="BlockFactorGeneratorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The factory to use for creating presenters.</param>
    public BlockFactorGeneratorPresenter(
        BlockFactorGenerator model,
        IBlockFactorGeneratorView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory) : base(model, view, registry, presenterFactory)
    {
        this.view = view;
        block = model;
        view.OnChanged.ConnectTo(OnChanged);
        RefreshView();
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    protected override void RefreshView()
    {
        // This will happen once, when this is called from the base class
        // constructor.
        if (view is null || block is null)
            return;
        base.RefreshView();
        view.Populate(block.BlockType, block.BlockName);
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnChanged(IModelChange<BlockFactorGenerator> change)
    {
        // Apply the command and refresh the view.
        ICommand command = change.ToCommand(block);
        InvokeCommand(command);
    }
}
