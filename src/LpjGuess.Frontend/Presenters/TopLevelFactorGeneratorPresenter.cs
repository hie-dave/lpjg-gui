using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a top-level factor generator.
/// </summary>
public class TopLevelFactorGeneratorPresenter : PresenterBase<ITopLevelFactorGeneratorView>, IFactorGeneratorPresenter
{
    /// <summary>
    /// The model instance managed by this presenter.
    /// </summary>
    private readonly TopLevelFactorGenerator model;

    /// <inheritdoc />
    public IFactorGenerator Model => model;

    /// <inheritdoc />
    public IView View => view;

    /// <inheritdoc />
    public string Name => model.Name;

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGeneratorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    public TopLevelFactorGeneratorPresenter(TopLevelFactorGenerator model, ITopLevelFactorGeneratorView view) : base(view)
    {
        this.model = model;
        view.OnChanged.ConnectTo(OnChanged);
        view.OnAddValue.ConnectTo(OnAddValue);
        RefreshView();
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    private void RefreshView()
    {
        view.Populate(model.Name, model.Values);
    }

    /// <summary>
    /// Handle the user clicking the "Add Value" button.
    /// </summary>
    private void OnAddValue()
    {
        // Just add a new empty value to the end of the list.
        OnChanged(new ModelChangeEventArgs<TopLevelFactorGenerator, List<string>>(
            m => m.Values,
            (f, values) => f.Values = values,
            model.Values.Append(string.Empty).ToList()));
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnChanged(IModelChange<TopLevelFactorGenerator> change)
    {
        // Apply the command and refresh the view.
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
        RefreshView();
    }
}
