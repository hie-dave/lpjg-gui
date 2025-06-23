using System.Globalization;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using static LpjGuess.Core.Extensions.EnumerableExtensions;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a discrete values view.
/// </summary>
public class DiscreteValuesPresenter : PresenterBase<IDiscreteValuesView>, IValueGeneratorPresenter
{
    /// <summary>
    /// The model object.
    /// </summary>
    private IValueGenerator model;

    /// <summary>
    /// Called when the data type has been changed by the user. The event
    /// parameter is the new value generator instance.
    /// </summary>
    public Event<IValueGenerator> OnTypeChanged { get; private init; }

    /// <inheritdoc />
    public IValueGenerator Model => model;

    /// <inheritdoc />
    public IView View => view;

    /// <summary>
    /// Create a new <see cref="DiscreteValuesPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    public DiscreteValuesPresenter(IValueGenerator model, IDiscreteValuesView view) : base(view)
    {
        this.model = model;
        OnTypeChanged = new Event<IValueGenerator>();

        view.OnAddValue.ConnectTo(OnAddValue);
        view.OnRemoveValue.ConnectTo(OnRemoveValue);
        view.OnChanged.ConnectTo(OnChanged);

        RefreshView();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnTypeChanged.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    protected override void InvokeCommand(ICommand command)
    {
        base.InvokeCommand(command);
        RefreshView();
    }

    /// <summary>
    /// Refresh the contents of the view with the current contents of the model.
    /// </summary>
    private void RefreshView()
    {
        view.Populate(model.GenerateStrings(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Called when the user has changed the values.
    /// </summary>
    /// <param name="enumerable">The new values.</param>
    private void OnChanged(IEnumerable<string> enumerable)
    {
        // Determine the type of the data by checking the values. If all values
        // are valid integers, use an integer type. If all values are valid
        // doubles, use a double type. Otherwise use a string type.
        if (enumerable.TrySelect(int.TryParse, out IEnumerable<int>? ints))
            OnChanged(ints);
        else if (enumerable.TrySelect(double.TryParse, out IEnumerable<double>? doubles))
            OnChanged(doubles);
        else
            OnChanged<string>(enumerable);
    }

    /// <summary>
    /// Handle the user changing the values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="values">The new values.</param>
    private void OnChanged<T>(IEnumerable<T> values)
    {
        if (model is not DiscreteValues<T> typedModel)
        {
            // Model is currently a differently-typed object. Need to
            // construct a new DiscreteValues<T> instance and raise the
            // OnTypeChanged event.
            typedModel = new DiscreteValues<T>(values);
            OnTypeChanged.Invoke(typedModel);
            model = typedModel;
            return;
        }

        // Model is already a DiscreteValues<T> instance. Update the values.
        PropertyChangeCommand<DiscreteValues<T>, List<T>> command = new(
            typedModel,
            typedModel.Values,
            values.ToList(),
            (m, v) => m.Values = v
        );
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the user has clicked the "Remove Value" button.
    /// </summary>
    /// <param name="index">The index of the value to remove.</param>
    private void OnRemoveValue(int index)
    {
        switch (model)
        {
            case DiscreteValues<string> stringModel:
                OnRemoveValue(stringModel, index);
                break;
            case DiscreteValues<int> intModel:
                OnRemoveValue(intModel, index);
                break;
            case DiscreteValues<double> doubleModel:
                OnRemoveValue(doubleModel, index);
                break;
            default:
                throw new InvalidOperationException($"Unknown model type: {model.GetType().Name}");
        }
    }

    /// <summary>
    /// Remove a value from the model.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="model">The model to remove the value from.</param>
    /// <param name="index">The index of the value to remove.</param>
    private void OnRemoveValue<T>(DiscreteValues<T> model, int index)
    {
        List<T> values = model.Values.ToList();
        values.RemoveAt(index);

        PropertyChangeCommand<DiscreteValues<T>, List<T>> command = new(
            model,
            model.Values,
            values,
            (m, v) => m.Values = v
        );
        InvokeCommand(command);
    }

    /// <summary>
    /// Called when the user has clicked the "Add Value" button.
    /// </summary>
    private void OnAddValue()
    {
        switch (model)
        {
            case DiscreteValues<string> stringModel:
                OnAddValueTo(stringModel);
                break;
            case DiscreteValues<int> intModel:
                OnAddValueTo(intModel);
                break;
            case DiscreteValues<double> doubleModel:
                OnAddValueTo(doubleModel);
                break;
            default:
                throw new InvalidOperationException($"Unknown model type: {model.GetType().Name}");
        }
    }

    /// <summary>
    /// Add a default value to the specified model.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="model">The model to add the value to.</param>
    private void OnAddValueTo<T>(DiscreteValues<T> model)
    {
        T newValue = CreateNewValue<T>();
        PropertyChangeCommand<DiscreteValues<T>, List<T>> command = new(
            model,
            model.Values,
            model.Values.Append(newValue).ToList(),
            (m, v) => m.Values = v
        );
        InvokeCommand(command);
    }

    /// <summary>
    /// Create a default value to be added to the list of values when the user
    /// clicks "add value".
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The default value.</returns>
    private static T CreateNewValue<T>()
    {
        if (typeof(T) == typeof(string))
            return (T)(object)string.Empty;
        return default!;
    }
}
