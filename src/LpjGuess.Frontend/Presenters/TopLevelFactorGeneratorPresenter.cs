using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a top-level factor generator.
/// </summary>
public class TopLevelFactorGeneratorPresenter : 
    PresenterBase<ITopLevelFactorGeneratorView, TopLevelFactorGenerator>,
    IFactorGeneratorPresenter<ITopLevelFactorGeneratorView, TopLevelFactorGenerator>
{
    /// <summary>
    /// The presenter responsible for managing the values of the factor
    /// generator.
    /// </summary>
    private IValueGeneratorPresenter? valuesPresenter;

    /// <inheritdoc />
    public TopLevelFactorGenerator Model => model;

    /// <inheritdoc />
    public IView View => view;

    /// <inheritdoc />
    public string Name => model.Name;

    /// <inheritdoc />
    public Event<string> OnRenamed { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGeneratorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public TopLevelFactorGeneratorPresenter(
        TopLevelFactorGenerator model,
        ITopLevelFactorGeneratorView view,
        ICommandRegistry registry) : base(view, model, registry)
    {
        OnRenamed = new Event<string>();
        valuesPresenter = null;
        view.OnChanged.ConnectTo(OnChanged);
        view.OnValuesTypeChanged.ConnectTo(OnValuesTypeChanged);
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
    protected virtual void RefreshView()
    {
        IValueGeneratorPresenter presenter = CreateValuesPresenter(model.Values);
        presenter.OnTypeChanged.ConnectTo(OnGeneratorTypeChanged);
        view.Populate(model.Name, GetGeneratorType(model.Values), presenter.View);

        if (valuesPresenter != null)
            valuesPresenter.Dispose();
        valuesPresenter = presenter;
    }

    /// <summary>
    /// Handle a change to the value generator type.
    /// </summary>
    /// <param name="generator">The new value generator.</param>
    private void OnGeneratorTypeChanged(IValueGenerator generator)
    {
        PropertyChangeCommand<TopLevelFactorGenerator, IValueGenerator> command = new(
            model,
            model.Values,
            generator,
            (m, v) => m.Values = v
        );
        InvokeCommand(command);
    }

    /// <summary>
    /// Create a value generator presenter for the given value generator.
    /// </summary>
    /// <param name="values">The value generator to create a presenter for.</param>
    /// <returns>The presenter.</returns>
    /// <exception cref="NotImplementedException">Thrown if the value generator type is not supported.</exception>
    private IValueGeneratorPresenter CreateValuesPresenter(IValueGenerator values)
    {
        // Handle discrete value generators.
        if (values is DiscreteValues<string> stringValues)
            return new DiscreteValuesPresenter(stringValues, new DiscreteValuesView());
        if (values is DiscreteValues<double> doubleValues)
            return new DiscreteValuesPresenter(doubleValues, new DiscreteValuesView());
        if (values is DiscreteValues<int> intValues)
            return new DiscreteValuesPresenter(intValues, new DiscreteValuesView());

        // Handle range-based value generators.
        if (values is RangeGenerator<double> doubleRange)
            return new RangeValuesPresenter(doubleRange, new RangeValuesView());
        if (values is RangeGenerator<int> intRange)
            return new RangeValuesPresenter(intRange, new RangeValuesView());

        throw new NotImplementedException($"Unknown value generator type: {values.GetType()}");
    }

    /// <summary>
    /// Create a default value generator for the given type.
    /// </summary>
    /// <param name="type">The type of value generator to create.</param>
    /// <returns>The default value generator.</returns>
    /// <exception cref="ArgumentException">Thrown if the type is not supported.</exception>
    private static IValueGenerator CreateDefaultGenerator(ValueGeneratorType type)
    {
        switch (type)
        {
            case ValueGeneratorType.Discrete:
                return new DiscreteValues<string>([]);
            case ValueGeneratorType.Range:
                return new RangeGenerator<int>(0, 5, 1);
            default:
                throw new ArgumentException("Invalid value generator type.");
        }
    }

    /// <summary>
    /// Get the type of value generator.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>The type of value generator.</returns>
    /// <exception cref="ArgumentException">Thrown if the generator type is not supported.</exception>
    private static ValueGeneratorType GetGeneratorType(IValueGenerator generator)
    {
        Type type = generator.GetType();
        if (type.IsGenericType)
        {
            Type genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(DiscreteValues<>))
                return ValueGeneratorType.Discrete;
            if (genericType == typeof(RangeGenerator<>))
                return ValueGeneratorType.Range;
        }
        throw new ArgumentException("Invalid value generator type.");
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
    }

    /// <summary>
    /// Handle a change to the value generator type.
    /// </summary>
    /// <param name="type">The new value generator type.</param>
    private void OnValuesTypeChanged(ValueGeneratorType type)
    {
        IValueGenerator generator = CreateDefaultGenerator(type);
        PropertyChangeCommand<TopLevelFactorGenerator, IValueGenerator> command = new(
            model,
            model.Values,
            generator,
            (m, v) => m.Values = v
        );
        InvokeCommand(command);
    }
}
