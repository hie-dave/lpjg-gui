using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Dialogs;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a factorial view.
/// </summary>
public class FactorialPresenter : PresenterBase<IFactorialView, FactorialGenerator>, IFactorialPresenter
{
    /// <summary>
    /// User-facing description of a top-level factor.
    /// </summary>
    private const string topLevelFactorTitle = "Top-Level";

    /// <summary>
    /// User-facing description of a block factor.
    /// </summary>
    private const string blockFactorTitle = "Block";

    /// <summary>
    /// User-facing description of a simple factor.
    /// </summary>
    private const string simpleFactorTitle = "Manual";

    /// <summary>
    /// The presenters responsible for managing the factors of the factorial.
    /// </summary>
    private List<IFactorGeneratorPresenter> presenters;

    /// <inheritdoc />
    public Event OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="FactorialPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present the factorial on.</param>
    /// <param name="view">The view to present the factorial on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public FactorialPresenter(
        FactorialGenerator model,
        IFactorialView view,
        ICommandRegistry registry) : base(view, model, registry)
    {
        presenters = [];
        view.OnAddFactor.ConnectTo(OnAddFactor);
        view.OnRemoveFactor.ConnectTo(OnRemoveFactor);
        OnChanged = new Event();
        view.OnChanged.ConnectTo(OnViewChanged);
    }

    /// <inheritdoc />
    public void Refresh()
    {
        var newPresenters = model.Factors.Select(CreateFactorPresenter).ToList();
        var views = newPresenters.Select(CreateValueView).ToList();
        view.Populate(model.FullFactorial, views);

        foreach (IFactorGeneratorPresenter presenter in presenters)
            presenter.Dispose();

        presenters = newPresenters;
        foreach (IFactorGeneratorPresenter presenter in presenters)
            presenter.OnRenamed.ConnectTo(n => OnFactorRenamed(n, presenter.GetView()));
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    protected override void InvokeCommand(ICommand command)
    {
        base.InvokeCommand(command);
        OnChanged.Invoke();
    }

    private IValueGeneratorView CreateValueView(IFactorGeneratorPresenter presenter)
    {
        const int maxValues = 1000;
        IEnumerable<string> values = presenter.Model.Generate().Take(maxValues).Select(f => f.GetName());
        return new ValueGeneratorView(presenter.Model.Name, presenter.Model.NumFactors() > maxValues, values, presenter.GetView());
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
            BlockFactorGeneratorPresenter presenter = new(blockFactorGenerator, view);
            return presenter;
        }

        if (factor is TopLevelFactorGenerator topLevelFactorGenerator)
        {
            TopLevelFactorGeneratorView view = new TopLevelFactorGeneratorView();
            TopLevelFactorGeneratorPresenter presenter = new(topLevelFactorGenerator, view);
            return presenter;
        }
        if (factor is SimpleFactorGenerator simpleGenerator)
        {
            SimpleFactorGeneratorView view = new SimpleFactorGeneratorView();
            SimpleFactorGeneratorPresenter presenter = new(simpleGenerator, view, registry);
            return presenter;
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
        if (factorType == topLevelFactorTitle)
            return new TopLevelFactorGenerator("wateruptake", generator);
        if (factorType == blockFactorTitle)
            return new BlockFactorGenerator("pft", "TeBE", "sla", generator);
        if (factorType == simpleFactorTitle)
            return new SimpleFactorGenerator("TODO: think of a better default name here", []);

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
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the user wants to add a new factor.
    /// </summary>
    private void OnAddFactor()
    {
		NameAndDescription[] factorTypes = [
            new NameAndDescription(topLevelFactorTitle, "Override a single top-level parameter (e.g. wateruptake)"),
            new NameAndDescription(blockFactorTitle, "Override a single block (e.g. PFT)-level parameter (e.g. sla)"),
            new NameAndDescription(simpleFactorTitle, "Manually configure factors consisting of one or more parameters")
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
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    private void OnViewChanged(IModelChange<FactorialGenerator> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }
}
