using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;
using LpjGuess.Frontend.Views.Dialogs;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a factorial view.
/// </summary>
[RegisterPresenter(typeof(FactorialGenerator), typeof(IFactorialPresenter))]
public class FactorialPresenter : PresenterBase<IFactorialView, FactorialGenerator>, IFactorialPresenter
{
    /// <summary>
    /// User-facing description of a top-level factor.
    /// </summary>
    private const string topLevelFactorTitle = "Global parameter";

    /// <summary>
    /// User-facing description of a block factor.
    /// </summary>
    private const string blockFactorTitle = "Block parameter";

    /// <summary>
    /// User-facing description of a simple factor.
    /// </summary>
    private const string simpleFactorTitle = "Multi-parameter scenarios";

    /// <summary>
    /// The presenter factory to use for creating value generator presenters.
    /// </summary>
    private readonly IPresenterFactory presenterFactory;
    private readonly IInstructionFilesProvider instructionFilesProvider;
    private IReadOnlyList<string>? selectedInstructionFiles;

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
    /// <param name="presenterFactory">The presenter factory to use for creating value generator presenters.</param>
    /// <param name="instructionFilesProvider">Provider used to discover parameter targets.</param>
    public FactorialPresenter(
        FactorialGenerator model,
        IFactorialView view,
        ICommandRegistry registry,
        IPresenterFactory presenterFactory,
        IInstructionFilesProvider instructionFilesProvider) : base(view, model, registry)
    {
        presenters = [];
        this.presenterFactory = presenterFactory;
        this.instructionFilesProvider = instructionFilesProvider;
        view.OnAddFactor.ConnectTo(OnAddFactor);
        view.OnRemoveFactor.ConnectTo(OnRemoveFactor);
        OnChanged = new Event();
        view.OnChanged.ConnectTo(OnViewChanged);
        Refresh();
    }

    /// <inheritdoc />
    public void SetInstructionFiles(IEnumerable<string> instructionFiles)
    {
        selectedInstructionFiles = instructionFiles.ToList();
        ApplyTargetSuggestions(presenters);
    }

    /// <inheritdoc />
    public void Refresh()
    {
        var newPresenters = model.Factors.Select(CreateFactorPresenter).ToList();
        ApplyTargetSuggestions(newPresenters);
        var views = newPresenters.Select(CreateValueView).ToList();
        view.Populate(model.FullFactorial, views);

        foreach (IFactorGeneratorPresenter presenter in presenters)
            presenter.Dispose();

        presenters = newPresenters;
        foreach (IFactorGeneratorPresenter presenter in presenters)
        {
            presenter.OnRenamed.ConnectTo(n => OnFactorRenamed(n, presenter.GetView()));
            presenter.OnChanged.ConnectTo(() => OnFactorChanged(presenter));
        }
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
        Refresh();
    }

    private IValueGeneratorView CreateValueView(IFactorGeneratorPresenter presenter)
    {
        const int maxValues = 1000;
        IEnumerable<string> generatedValues = presenter.Model switch
        {
            TopLevelFactorGenerator parameter =>
                parameter.Values.GenerateStrings(CultureInfo.InvariantCulture),
            _ => presenter.Model.Generate().Select(factor => factor.GetName())
        };
        List<string> values = generatedValues.Take(maxValues).ToList();
        int count = Math.Max(0, presenter.Model.NumFactors());
        string valueSummary = values.Count == 0
            ? "No values configured"
            : string.Join(", ", values.Take(4)) + (count > 4 ? ", …" : string.Empty);

        string kind;
        string target;
        switch (presenter.Model)
        {
            case BlockFactorGenerator block:
                kind = "Block parameter";
                target = ParameterTarget.Block(block.BlockType, block.BlockName, block.Name).DisplayName;
                break;
            case TopLevelFactorGenerator parameter:
                kind = "Global parameter";
                target = ParameterTarget.TopLevel(parameter.Name).DisplayName;
                break;
            case SimpleFactorGenerator scenarios:
                kind = "Scenario set";
                List<string> targets = scenarios.Levels
                    .SelectMany(level => level.GetParameterOverrides())
                    .Select(change => change.Target.DisplayName)
                    .Distinct()
                    .ToList();
                target = targets.Count == 0
                    ? "No parameters configured"
                    : string.Join(", ", targets.Take(4)) +
                        (targets.Count > 4 ? ", …" : string.Empty);
                break;
            default:
                kind = "Variation";
                target = string.Empty;
                break;
        }

        return new ValueGeneratorView(
            presenter.Model.Name,
            kind,
            target,
            valueSummary,
            count,
            count <= maxValues,
            values,
            presenter.GetView());
    }

    private void OnFactorChanged(IFactorGeneratorPresenter presenter)
    {
        view.UpdateFactor(CreateValueView(presenter));
        OnChanged.Invoke();
    }

    /// <summary>
    /// Called when the name of a factor has been changed by the user.
    /// </summary>
    /// <param name="name">The new name of the factor.</param>
    /// <param name="view">The view of the factor generator being renamed.</param>
    private void OnFactorRenamed(string name, IView view)
    {
        IFactorGeneratorPresenter? presenter = presenters
            .FirstOrDefault(candidate => ReferenceEquals(candidate.GetView(), view));
        if (presenter is null)
            return;

        this.view.UpdateFactor(CreateValueView(presenter));
        OnChanged.Invoke();
    }

    /// <summary>
    /// Create a presenter for a factor generator.
    /// </summary>
    /// <param name="factor">The factor generator to create a presenter for.</param>
    /// <returns>A tuple containing the presenter and view.</returns>
    private IFactorGeneratorPresenter CreateFactorPresenter(IFactorGenerator factor)
    {
        return presenterFactory.CreatePresenter<IFactorGeneratorPresenter>(factor);
    }

    /// <summary>
    /// Create a new factor generator.
    /// </summary>
    /// <param name="factorType">The type of factor to create.</param>
    /// <returns>The new factor generator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the factor type is unknown.</exception>
    private IFactorGenerator CreateFactor(string factorType)
    {
        if (factorType == topLevelFactorTitle)
            return new TopLevelFactorGenerator(string.Empty, new DiscreteValues<string>([string.Empty]));
        if (factorType == blockFactorTitle)
            return new BlockFactorGenerator(
                "pft",
                string.Empty,
                string.Empty,
                new DiscreteValues<string>([string.Empty]));
        if (factorType == simpleFactorTitle)
            return new SimpleFactorGenerator(
                "Scenario set",
                [new CompositeFactor([new TopLevelParameter(string.Empty, string.Empty)])
                {
                    Name = "Scenario 1"
                }]);

        throw new InvalidOperationException($"Unknown factor type: {factorType}");
    }

    /// <summary>
    /// Called when the user wants to delete the factor with the specified name.
    /// </summary>
    /// <param name="view">The view of the factor to delete.</param>
    private void OnRemoveFactor(IView view)
    {
        List<IFactorGenerator> filtered = presenters
            .Where(p => !ReferenceEquals(p.GetView(), view))
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
            new NameAndDescription(topLevelFactorTitle, "Vary one global instruction-file parameter"),
            new NameAndDescription(blockFactorTitle, "Vary one parameter inside a named PFT, stand, or other block"),
            new NameAndDescription(simpleFactorTitle, "Define levels which each change one or more parameters")
		];
		string prompt = "What do you want to vary?";
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
        AddFactor(CreateFactor(factorType));
    }

    private void AddFactor(IFactorGenerator factor)
    {
        var change = new ModelChangeEventArgs<FactorialGenerator, IEnumerable<IFactorGenerator>>(
            m => m.Factors,
            (m, v) => m.Factors = v,
            model.Factors.Append(factor).ToList()
        );
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }

    private void ApplyTargetSuggestions(IEnumerable<IFactorGeneratorPresenter> targetPresenters)
    {
        IReadOnlyList<ParameterTarget> targets = DiscoverTargets();
        foreach (IFactorGeneratorPresenter presenter in targetPresenters)
            presenter.SetTargetSuggestions(targets);
    }

    private IReadOnlyList<ParameterTarget> DiscoverTargets()
    {
        HashSet<ParameterTarget> targets = [];
        IEnumerable<string> files = selectedInstructionFiles ??
            instructionFilesProvider.GetInstructionFiles().ToList();
        foreach (string file in files.Where(File.Exists))
        {
            try
            {
                InstructionFileParser parser = InstructionFileParser.FromFile(file);
                foreach (string parameter in parser.GetTopLevelParameterNames())
                    targets.Add(ParameterTarget.TopLevel(parameter));

                foreach ((string blockType, string blockName) in parser.GetBlocks())
                {
                    foreach (string parameter in parser.GetBlockParameterNames(blockType, blockName))
                        targets.Add(ParameterTarget.Block(blockType, blockName, parameter));
                }
            }
            catch
            {
                // Target discovery is best-effort. Parse errors remain visible
                // through the instruction-file editor and run path.
            }
        }

        return targets
            .OrderBy(target => target.DisplayName)
            .ToList();
    }

    private void OnViewChanged(IModelChange<FactorialGenerator> change)
    {
        ICommand command = change.ToCommand(model);
        InvokeCommand(command);
    }
}
