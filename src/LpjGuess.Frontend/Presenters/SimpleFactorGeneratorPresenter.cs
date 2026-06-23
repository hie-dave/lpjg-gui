using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Dialogs;
using static LpjGuess.Frontend.Utility.FactorHelpers;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a simple factor generator.
/// </summary>
[RegisterPresenter(typeof(SimpleFactorGenerator), typeof(IFactorGeneratorPresenter))]
public class SimpleFactorGeneratorPresenter : PresenterBase<ISimpleFactorGeneratorView, SimpleFactorGenerator>, IFactorGeneratorPresenter
{
    /// <summary>
    /// The factory to use for creating presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The presenters responsible for managing the factor levels.
    /// </summary>
    private List<IFactorPresenter> factorPresenters;
    private IReadOnlyList<ParameterTarget> targetSuggestions;

    /// <inheritdoc />
    public string Name => model.Name;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <inheritdoc />
    public Event OnChanged { get; private init; }

    /// <inheritdoc />
    IFactorGenerator IPresenter<IFactorGenerator>.Model => model;

    /// <summary>
    /// Create a new <see cref="SimpleFactorGeneratorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The factory to use for creating presenters.</param>
    public SimpleFactorGeneratorPresenter(
        SimpleFactorGenerator model,
        ISimpleFactorGeneratorView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory) : base(view, model, registry)
    {
        this.presenterFactory = presenterFactory;
        factorPresenters = new List<IFactorPresenter>();
        targetSuggestions = [];
        OnRenamed = new Event<string>();
        OnChanged = new Event();
        view.OnChanged.ConnectTo(OnModelChanged);
        view.OnAddLevel.ConnectTo(OnAddLevel);
        view.OnRemoveLevel.ConnectTo(OnRemoveLevel);
        RefreshView();
    }

    /// <inheritdoc />
    public void SetTargetSuggestions(IEnumerable<ParameterTarget> targets)
    {
        targetSuggestions = targets.ToList();
        foreach (IFactorPresenter presenter in factorPresenters)
            presenter.SetTargetSuggestions(targetSuggestions);
    }

    /// <inheritdoc />
    protected override void InvokeCommand(ICommand command)
    {
        string oldName = model.Name;
        base.InvokeCommand(command);
        RefreshView();
        if (oldName != model.Name)
            OnRenamed.Invoke(model.Name);
        else
            OnChanged.Invoke();
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    private void RefreshView()
    {
        List<IFactorPresenter> presenters = model.Levels.Select(CreateFactorPresenter).ToList();
        presenters.ForEach(presenter => presenter.SetTargetSuggestions(targetSuggestions));
        view.Populate(model.Name, presenters.Select((presenter, index) =>
            new NamedView(
                presenter.GetView(),
                string.IsNullOrWhiteSpace(presenter.Model.GetName())
                    ? $"Scenario {index + 1}"
                    : presenter.Model.GetName())));
        presenters.ForEach(p => p.OnRenamed.ConnectTo(n => OnChildNameChanged(n, p)));
        presenters.ForEach(p => p.OnChanged.ConnectTo(OnChanged));

        factorPresenters.ForEach(p => p.Dispose());
        factorPresenters = presenters;
    }

    private void OnChildNameChanged(string name, IFactorPresenter presenter)
    {
        view.Rename(presenter.GetView(), name);
        OnChanged.Invoke();
    }

    /// <summary>
    /// Create a factor presenter for the given factor.
    /// </summary>
    /// <param name="factor">The factor to create a presenter for.</param>
    /// <returns>The presenter.</returns>
    private IFactorPresenter CreateFactorPresenter(IFactor factor)
    {
        return presenterFactory.CreatePresenter<IFactorPresenter>(factor);
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnModelChanged(IModelChange<SimpleFactorGenerator> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    /// <summary>
    /// Handle the user adding a new level.
    /// </summary>
    private void OnAddLevel()
    {
        int number = model.Levels.Count() + 1;
        IFactor factor = new CompositeFactor([
            new TopLevelParameter(string.Empty, string.Empty)
        ]) { Name = $"Scenario {number}" };
        PropertyChangeCommand<SimpleFactorGenerator, IEnumerable<IFactor>> command = new(
            model,
            model.Levels,
            // Ensure we use greedy evaluation.
            model.Levels.Append(factor).ToList(),
            (f, levels) => f.Levels = levels
        );
        InvokeCommand(command);
    }

    /// <summary>
    /// Handle the user removing a level.
    /// </summary>
    /// <param name="view">The view of the level to remove.</param>
    private void OnRemoveLevel(IView view)
    {
        ICommand command = new PropertyChangeCommand<SimpleFactorGenerator, IEnumerable<IFactor>>(
            model,
            model.Levels,
            factorPresenters.Where(p => p.GetView() != view).Select(p => p.Model).ToList(),
            (f, levels) => f.Levels = levels
        );
        InvokeCommand(command);
    }
}
