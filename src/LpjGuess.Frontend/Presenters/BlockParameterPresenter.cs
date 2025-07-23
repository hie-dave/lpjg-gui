using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a concrete block parameter.
/// </summary>
[RegisterPresenter(typeof(BlockParameter), typeof(IFactorPresenter))]
public class BlockParameterPresenter : TopLevelParameterPresenter, IFactorPresenter
{
    /// <summary>
    /// The model instance managed by this presenter.
    /// </summary>
    private readonly BlockParameter block;

    /// <summary>
    /// The view instance managed by this presenter.
    /// </summary>
    private readonly IBlockParameterView blockView;

    /// <summary>
    /// Create a new <see cref="BlockParameterPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public BlockParameterPresenter(
        BlockParameter model,
        IBlockParameterView view,
        ICommandRegistry registry)
        : base(model, view, registry)
    {
        block = model;
        blockView = view;
        view.OnChanged.ConnectTo(OnBlockParameterChanged);
        RefreshView();
    }

    /// <inheritdoc />
    protected override void RefreshView()
    {
        // This is called from the base class constructor, at which point the
        // model and view will be null.
        if (block == null || blockView == null)
            return;
        blockView.Populate(block.Name, block.BlockType, block.BlockName, block.Value);
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnBlockParameterChanged(IModelChange<BlockParameter> change)
    {
        ICommand command = change.ToCommand(block);
        InvokeCommand(command);
    }
}
