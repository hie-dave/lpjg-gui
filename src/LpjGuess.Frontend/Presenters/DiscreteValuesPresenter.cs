using System.Globalization;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Attributes;
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
[RegisterGenericType(typeof(string), typeof(int), typeof(double))]
public class DiscreteValuesPresenter<T> : PresenterBase<IDiscreteValuesView, DiscreteValues<T>>, IValueGeneratorPresenter, IPresenter<IDiscreteValuesView, DiscreteValues<T>>
{
    /// <summary>
    /// Called when the data type has been changed by the user. The event
    /// parameter is the new value generator instance.
    /// </summary>
    public Event<IValueGenerator> OnTypeChanged { get; private init; }

    /// <inheritdoc />
    public IView View => view;

    /// <summary>
    /// Create a new <see cref="DiscreteValuesPresenter{T}"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public DiscreteValuesPresenter(
        DiscreteValues<T> model,
        IDiscreteValuesView view,
        ICommandRegistry registry) : base(view, model, registry)
    {
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
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="values">The new values.</param>
    private void OnChanged<TValue>(IEnumerable<TValue> values)
    {
        if (model is not DiscreteValues<TValue> typedModel)
        {
            // Model is currently a differently-typed object. Need to
            // construct a new DiscreteValues<T> instance and raise the
            // OnTypeChanged event.
            typedModel = new DiscreteValues<TValue>(values);
            OnTypeChanged.Invoke(typedModel);
            return;
        }

        // Model is already a DiscreteValues<T> instance. Update the values.
        PropertyChangeCommand<DiscreteValues<TValue>, List<TValue>> command = new(
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
        T newValue = CreateNewValue();
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
    /// <returns>The default value.</returns>
    private static T CreateNewValue()
    {
        if (typeof(T) == typeof(string))
            return (T)(object)string.Empty;
        return default!;
    }
}
