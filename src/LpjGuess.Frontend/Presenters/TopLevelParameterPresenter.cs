using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a concrete top-level parameter.
/// </summary>
public class TopLevelParameterPresenter : PresenterBase<ITopLevelParameterView, TopLevelParameter>, IFactorPresenter
{
    /// <inheritdoc />
    public IFactor Model => model;

    /// <inheritdoc />
    public IView View => view;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelParameterPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public TopLevelParameterPresenter(
        TopLevelParameter model,
        ITopLevelParameterView view,
        ICommandRegistry registry)
        : base(view, model, registry)
    {
        OnRenamed = new Event<string>();
        view.OnChanged.ConnectTo(OnChanged);
        RefreshView();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnRenamed.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    protected override void InvokeCommand(ICommand command)
    {
        string oldName = model.GetName();
        base.InvokeCommand(command);
        RefreshView();
        string newName = model.GetName();
        if (oldName != newName)
            OnRenamed.Invoke(newName);
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    protected virtual void RefreshView()
    {
        view.Populate(model.Name, model.Value);
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    protected virtual void OnChanged(IModelChange<TopLevelParameter> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }
}
