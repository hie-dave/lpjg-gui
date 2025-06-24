using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a concrete block parameter.
/// </summary>
public class BlockParameterPresenter : TopLevelParameterPresenter, IFactorPresenter
{
    /// <summary>
    /// The view instance managed by this presenter.
    /// </summary>
    private new readonly IBlockParameterView view;

    /// <summary>
    /// The model instance managed by this presenter.
    /// </summary>
    private readonly BlockParameter model;

    /// <summary>
    /// Create a new <see cref="BlockParameterPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    public BlockParameterPresenter(BlockParameter model, IBlockParameterView view)
        : base(model, view)
    {
        this.model = model;
        this.view = view;
        view.OnChanged.ConnectTo(OnBlockParameterChanged);
        RefreshView();
    }

    /// <inheritdoc />
    protected override void RefreshView()
    {
        // This is called from the base class constructor, at which point the
        // model and view will be null.
        if (model == null || view == null)
            return;
        view.Populate(model.Name, model.BlockType, model.BlockName, model.Value);
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnBlockParameterChanged(IModelChange<BlockParameter> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }
}
