using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.DependencyInjection;
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
/// A presenter for a composite factor.
/// </summary>
public class CompositeFactorPresenter : PresenterBase<ICompositeFactorView, CompositeFactor>, IFactorPresenter
{
    /// <summary>
    /// The factory to use for creating presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;

    /// <summary>
    /// The presenters responsible for managing the individual factors.
    /// </summary>
    private List<IFactorPresenter> factorPresenters;

    /// <inheritdoc />
    IFactor IPresenter<IFactor>.Model => model;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="CompositeFactorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    /// <param name="presenterFactory">The factory to use for creating presenters.</param>
    public CompositeFactorPresenter(
        CompositeFactor model,
        ICompositeFactorView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory) : base(view, model, registry)
    {
        factorPresenters = new List<IFactorPresenter>();
        OnRenamed = new Event<string>();
        this.presenterFactory = presenterFactory;
        view.OnChanged.ConnectTo(OnChanged);
        view.OnAddFactor.ConnectTo(OnAddFactor);
        view.OnRemoveFactor.ConnectTo(OnRemoveFactor);
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
        if (oldName != model.GetName())
            OnRenamed.Invoke(model.GetName());
    }

    /// <summary>
    /// Populate the contents of the view with the current model state.
    /// </summary>
    private void RefreshView()
    {
        List<IFactorPresenter> presenters = model.Factors.Select(CreateFactorPresenter).ToList();
        view.Populate(model.GetName(), presenters.Select(p => new NamedView(p.GetView(), p.Model.GetName())));
        presenters.ForEach(p => p.OnRenamed.ConnectTo(_ => OnPresenterRenamed()));

        factorPresenters.ForEach(p => p.Dispose());
        factorPresenters = presenters;
    }

    /// <summary>
    /// Create a factor presenter for the given factor.
    /// </summary>
    /// <param name="factor">The factor to create a presenter for.</param>
    /// <returns>The presenter.</returns>
    private IFactorPresenter CreateFactorPresenter(IFactor factor)
    {
        return presenterFactory.CreatePresenter<IFactorPresenter, IFactor>(factor);
    }

    /// <summary>
    /// Called when the user has changed the model.
    /// In practice, this is probably not used currently because CompositeFactor
    /// doesn't really have much in the way of configurable properties (yet!).
    /// </summary>
    /// <param name="change">The change to the model.</param>
    private void OnChanged(IModelChange<CompositeFactor> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the user wants to remove a factor.
    /// </summary>
    /// <param name="name">The name of the factor view corresponding to the factor to be removed.</param>
    private void OnRemoveFactor(string name)
    {
        PropertyChangeCommand<CompositeFactor, IEnumerable<IFactor>> command =
            new PropertyChangeCommand<CompositeFactor, IEnumerable<IFactor>>(
                model,
                model.Factors,
                model.Factors.Where(f => f.GetName() != name).ToList(),
                (f, factors) => f.Factors = factors
            );
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the user wants to add a factor.
    /// </summary>
    private void OnAddFactor()
    {
        AskUserDialog.RunFor(
            [FactorType.TopLevel, FactorType.Block],
            GetFactorTypeName,
            GetFactorTypeDescription,
            "Select a Factor Type",
            "Add",
            OnAddFactor
        );
    }

    /// <summary>
    /// Called when the user wants to add a specific kind of factor.
    /// </summary>
    /// <param name="type">The type of factor to be added.</param>
    private void OnAddFactor(FactorType type)
    {
        IFactor factor = CreateDefaultFactor(type);
        PropertyChangeCommand<CompositeFactor, IEnumerable<IFactor>> command =
            new PropertyChangeCommand<CompositeFactor, IEnumerable<IFactor>>(
                model,
                model.Factors,
                model.Factors.Append(factor).ToList(),
                (f, factors) => f.Factors = factors
            );
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when a factor presenter's name changes.
    /// </summary>
    private void OnPresenterRenamed()
    {
        view.Rename(model.GetName());
        OnRenamed.Invoke(model.GetName());
    }
}
