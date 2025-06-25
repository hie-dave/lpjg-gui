using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
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
public class SimpleFactorGeneratorPresenter : PresenterBase<ISimpleFactorGeneratorView>, IFactorGeneratorPresenter
{
    /// <summary>
    /// The model to present.
    /// </summary>
    private readonly SimpleFactorGenerator model;

    /// <summary>
    /// The presenters responsible for managing the factor levels.
    /// </summary>
    private List<IFactorPresenter> factorPresenters;

    /// <inheritdoc />
    public IFactorGenerator Model => model;

    /// <inheritdoc />
    public string Name => model.Name;

    /// <inheritdoc />
    public IView View => view;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="SimpleFactorGeneratorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    public SimpleFactorGeneratorPresenter(SimpleFactorGenerator model, ISimpleFactorGeneratorView view) : base(view)
    {
        this.model = model;
        factorPresenters = new List<IFactorPresenter>();
        OnRenamed = new Event<string>();
        view.OnChanged.ConnectTo(OnChanged);
        view.OnAddLevel.ConnectTo(OnAddLevel);
        view.OnRemoveLevel.ConnectTo(OnRemoveLevel);
        RefreshView();
    }

    /// <inheritdoc />
    protected override void InvokeCommand(ICommand command)
    {
        string oldName = model.Name;
        base.InvokeCommand(command);
        RefreshView();
        if (oldName != model.Name)
            OnRenamed.Invoke(model.Name);
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    private void RefreshView()
    {
        List<IFactorPresenter> presenters = model.Levels.Select(CreateFactorPresenter).ToList();
        view.Populate(model.Name, presenters.Select(p => new NamedView(p.View, p.Model.GetName())));
        presenters.ForEach(p => p.OnRenamed.ConnectTo(n => view.Rename(p.View, n)));

        factorPresenters.ForEach(p => p.Dispose());
        factorPresenters = presenters;
    }

    /// <summary>
    /// Handle an arbitrary change to the model.
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnChanged(IModelChange<SimpleFactorGenerator> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    /// <summary>
    /// Handle the user adding a new level.
    /// </summary>
    private void OnAddLevel()
    {
        // Ask the user which kind of factor they want to add.
        AskUserDialog.RunFor(
            Enum.GetValues<FactorType>(),
            GetFactorTypeName,
            GetFactorTypeDescription,
            "Select a Factor Type",
            "Add",
            OnAddLevel);
    }

    /// <summary>
    /// Handle the user adding a new level.
    /// </summary>
    /// <param name="type">The type of factor to add.</param>
    private void OnAddLevel(FactorType type)
    {
        IFactor factor = CreateDefaultFactor(type);
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
    /// <param name="name">The name of the level to remove.</param>
    private void OnRemoveLevel(string name)
    {
        ICommand command = new PropertyChangeCommand<SimpleFactorGenerator, IEnumerable<IFactor>>(
            model,
            model.Levels,
            model.Levels.Where(f => f.GetName() != name).ToList(),
            (f, levels) => f.Levels = levels
        );
        InvokeCommand(command);
    }
}
