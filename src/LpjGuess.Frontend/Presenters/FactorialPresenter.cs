using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Dialogs;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a factorial view.
/// </summary>
public class FactorialPresenter : PresenterBase<IFactorialView>, IFactorialPresenter
{
    /// <summary>
    /// The model object.
    /// </summary>
    private FactorialGenerator model;

    /// <summary>
    /// User-facing description of a top-level factor.
    /// </summary>
    private const string topLevelFactorDescription = "Top-Level";

    /// <summary>
    /// User-facing description of a block factor.
    /// </summary>
    private const string blockFactorDescription = "Block";

    /// <summary>
    /// The presenters responsible for managing the factors of the factorial.
    /// </summary>
    private List<IFactorGeneratorPresenter> presenters;

    /// <inheritdoc />
    public Event<IModelChange<FactorialGenerator>> OnChanged => view.OnChanged;

    /// <summary>
    /// Create a new <see cref="FactorialPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present the factorial on.</param>
    /// <param name="view">The view to present the factorial on.</param>
    public FactorialPresenter(FactorialGenerator model, IFactorialView view) : base(view)
    {
        this.model = model;
        presenters = [];
        view.OnAddFactor.ConnectTo(OnAddFactor);
        view.OnRemoveFactor.ConnectTo(OnRemoveFactor);
    }

    /// <inheritdoc />
    public void Populate(FactorialGenerator factorial)
    {
        model = factorial;
        var newPresenters = factorial.Factors.Select(CreateFactorPresenter).ToList();
        var views = newPresenters.Select(p => new NamedView(p.View, p.Name)).ToList();
        view.Populate(factorial.FullFactorial, views);

        foreach (IFactorGeneratorPresenter presenter in presenters)
            presenter.Dispose();

        presenters = newPresenters;
        foreach (IFactorGeneratorPresenter presenter in presenters)
            presenter.OnRenamed.ConnectTo(n => OnFactorRenamed(n, presenter.View));
    }

    /// <summary>
    /// Called when the name of a factor has been changed by the user.
    /// </summary>
    /// <param name="name">The new name of the factor.</param>
    /// <param name="view">The view of the factor generator being renamed.</param>
    private void OnFactorRenamed(string name, IView view)
    {
        // Inform the view of the change in this model's name.
        this.view.RenameFactor(view, name);
    }

    /// <summary>
    /// Create a presenter for a factor generator.
    /// </summary>
    /// <param name="factor">The factor generator to create a presenter for.</param>
    /// <returns>A tuple containing the presenter and view.</returns>
    private IFactorGeneratorPresenter CreateFactorPresenter(IFactorGenerator factor)
    {
        if (factor is BlockFactorGenerator blockFactorGenerator)
        {
            BlockFactorGeneratorView view = new BlockFactorGeneratorView();
            BlockFactorGeneratorPresenter presenter = new BlockFactorGeneratorPresenter(blockFactorGenerator, view);
            return presenter;
        }

        if (factor is TopLevelFactorGenerator topLevelFactorGenerator)
        {
            TopLevelFactorGeneratorView view = new TopLevelFactorGeneratorView();
            TopLevelFactorGeneratorPresenter presenter = new TopLevelFactorGeneratorPresenter(topLevelFactorGenerator, view);
            return presenter;
        }
        if (factor is CompositeFactor compositeFactor)
        {
            throw new NotImplementedException("CompositeFactor is TBI");
            // CompositeFactorPresenter presenter = new CompositeFactorPresenter(compositeFactor);
            // return (presenter, presenter.View);
        }

        throw new InvalidOperationException($"Unknown factor type: {factor.GetType().Name}");
    }

    /// <summary>
    /// Create a new factor generator.
    /// </summary>
    /// <param name="factorType">The type of factor to create.</param>
    /// <returns>The new factor generator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the factor type is unknown.</exception>
    private IFactorGenerator CreateFactor(string factorType)
    {
        IValueGenerator generator = new DiscreteValues<string>([]);
        if (factorType == topLevelFactorDescription)
            return new TopLevelFactorGenerator("wateruptake", generator);
        if (factorType == blockFactorDescription)
            return new BlockFactorGenerator("pft", "TeBE", "sla", generator);

        // TBI: CompositeFactor.
        throw new InvalidOperationException($"Unknown factor type: {factorType}");
    }

    /// <summary>
    /// Called when the user wants to delete the factor with the specified name.
    /// </summary>
    /// <param name="name">Name of the factor.</param>
    private void OnRemoveFactor(string name)
    {
        List<IFactorGenerator> filtered = presenters
            .Where(p => p.Name != name)
            .Select(p => p.Model)
            .ToList();

        var change = new ModelChangeEventArgs<FactorialGenerator, IEnumerable<IFactorGenerator>>(
            m => m.Factors,
            (m, v) => m.Factors = v,
            filtered
        );
        OnChanged.Invoke(change);
    }

    /// <summary>
    /// Called when the user wants to add a new factor.
    /// </summary>
    private void OnAddFactor()
    {
		NameAndDescription[] factorTypes = [
			new NameAndDescription(topLevelFactorDescription, "Override a Top-level parameter (e.g. npatch)"),
			new NameAndDescription(blockFactorDescription, "Override a block (e.g. PFT)-level parameter (e.g. sla)")
		];
		string prompt = "Select a factor type";
		AskUserDialog dialog = new AskUserDialog(prompt, "Select", factorTypes);
		dialog.OnSelected.ConnectTo(OnFactorTypeSelected);
		dialog.Run();
    }

    /// <summary>
    /// Called when the user has selected a factor type.
    /// </summary>
    /// <param name="factorType">The type of factor to create.</param>
    private void OnFactorTypeSelected(string factorType)
    {
        IFactorGenerator factor = CreateFactor(factorType);
        var change = new ModelChangeEventArgs<FactorialGenerator, IEnumerable<IFactorGenerator>>(
            m => m.Factors,
            (m, v) => m.Factors = v,
            model.Factors.Append(factor).ToList()
        );
        OnChanged.Invoke(change);
    }
}
