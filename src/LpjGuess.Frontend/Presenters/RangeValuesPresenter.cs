using System.Globalization;
using System.Numerics;
using GObject;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Values;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a range values view.
/// </summary>
public class RangeValuesPresenter<T> : PresenterBase<IRangeValuesView, RangeGenerator<T>>, IValueGeneratorPresenter
    where T : struct, INumber<T>
{
    /// <inheritdoc />
    public IValueGenerator Model => model;

    /// <inheritdoc />
    public IView View => view;

    /// <inheritdoc />
    public Event<IValueGenerator> OnTypeChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="RangeValuesPresenter{T}"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public RangeValuesPresenter(
        RangeGenerator<T> model,
        IRangeValuesView view,
        ICommandRegistry registry)
        : base(view, model, registry)
    {
        OnTypeChanged = new Event<IValueGenerator>();

        view.OnStartChanged.ConnectTo(OnStartChanged);
        view.OnNChanged.ConnectTo(OnNChanged);
        view.OnStepChanged.ConnectTo(OnStepChanged);

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
    /// Refresh the view with the current contents of the model.
    /// </summary>
    private void RefreshView()
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        const int maxValues = 1000;
        IEnumerable<string> values = Model.GenerateStrings(culture).Take(maxValues);
        if (Model is RangeGenerator<double> doubleModel)
        {
            view.Populate(doubleModel.Start.ToString(culture), doubleModel.N, doubleModel.Step.ToString(culture), values);
            view.SetMoreValuesIndicator(doubleModel.N > maxValues);
        }
        else if (Model is RangeGenerator<int> intModel)
        {
            view.Populate(intModel.Start.ToString(culture), intModel.N, intModel.Step.ToString(culture), values);
            view.SetMoreValuesIndicator(intModel.N > maxValues);
        }
    }

    /// <summary>
    /// Check if the specified string is a valid double. This will return false
    /// for values which are valid integers (e.g. "3").
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="result">The parsed double.</param>
    /// <returns>True if the string is a valid double, false otherwise.</returns>
    private static bool IsDouble(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Check if the specified string is a valid integer.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="result">The parsed integer.</param>
    /// <returns>True if the string is a valid integer, false otherwise.</returns>
    private static bool IsInt(string value, out int result)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    /// <summary>
    /// Check if the specified double is an integer (e.g. 3.0).
    /// </summary>
    /// <param name="value">The double to check.</param>
    /// <returns>True if the double is an integer, false otherwise.</returns>
    private static bool IsInt(double value)
    {
        return value == (int)value;
    }

    /// <summary>
    /// Get the current start value, or a default value if none is set.
    /// </summary>
    /// <returns>The start value.</returns>
    private TValue GetStart<TValue>() where TValue : struct, INumber<TValue>
    {
        if (Model is RangeGenerator<double> dblModel)
            return (TValue)Convert.ChangeType(dblModel.Start, typeof(TValue));
        else if (Model is RangeGenerator<int> intModel)
            return (TValue)Convert.ChangeType(intModel.Start, typeof(TValue));
        return default;
    }

    /// <summary>
    /// Get the current number of values to generate, or a default value if none
    /// is set.
    /// </summary>
    /// <returns>The number of values to generate.</returns>
    private int GetN()
    {
        if (Model is IRangeGenerator rangeGenerator)
            return rangeGenerator.N;
        return 0;
    }

    /// <summary>
    /// Get the current step value, or a default value if none is set.
    /// </summary>
    /// <returns>The step value.</returns>
    private TValue GetStep<TValue>() where TValue : struct, INumber<TValue>
    {
        if (Model is RangeGenerator<double> dblModel)
            return (TValue)Convert.ChangeType(dblModel.Step, typeof(TValue));
        else if (Model is RangeGenerator<int> intModel)
            return (TValue)Convert.ChangeType(intModel.Step, typeof(TValue));
        return default;
    }

    /// <summary>
    /// Handle the user changing a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="newValue">The new value.</param>
    /// <param name="getter">A function to get the current value.</param>
    /// <param name="setter">A function to set the new value.</param>
    private void OnValueChanged<TValue>(
        TValue newValue,
        Func<RangeGenerator<TValue>, TValue> getter,
        Action<RangeGenerator<TValue>, TValue> setter) where TValue : struct, INumber<TValue>
    {
        if (Model is RangeGenerator<TValue> model)
        {
            // Model is already of the correct type.
            InvokeCommand(new PropertyChangeCommand<RangeGenerator<TValue>, TValue>(
                model,
                getter(model),
                newValue,
                setter));
        }
        else
        {
            // Model needs to be converted to a RangeGenerator<TValue> instance.

            // Attempt to copy current values.
            TValue start = GetStart<TValue>();
            int n = GetN();
            TValue step = GetStep<TValue>();

            // Create a new RangeGenerator<TValue> instance and set the new value.
            RangeGenerator<TValue> newModel = new RangeGenerator<TValue>(start, n, step);
            setter(newModel, newValue);

            // Emit a type changed event.
            OnTypeChanged.Invoke(newModel);
        }
    }

    /// <summary>
    /// Called when the user has changed the start value.
    /// </summary>
    /// <param name="value">The new start value.</param>
    private void OnStartChanged(string value)
    {
        // Check the data type of the value.
        // We need to check what kind of value was provided.
        if (IsDouble(value, out double doubleValue))
            OnValueChanged(doubleValue, (m) => m.Start, (m, v) => m.Start = v);
        else if (IsInt(value, out int integer))
        {
            if (Model is RangeGenerator<double> doubleModel && !IsInt(doubleModel.Step))
                // Step is a double, so we need to convert the start value to a
                // double.
                OnValueChanged((double)integer, (m) => m.Start, (m, v) => m.Start = v);
            else
                // Model is not a double model, or step is an integer and the
                // model may therefore be safely changed to an int model.
                OnValueChanged(integer, (m) => m.Start, (m, v) => m.Start = v);
        }
        else
            throw new InvalidOperationException($"Invalid start value: '{value}'. Must be a number");
    }

    /// <summary>
    /// Called when the user has changed the n value.
    /// </summary>
    /// <param name="n">The new n value.</param>
    private void OnNChanged(int n)
    {
        if (Model is IRangeGenerator rangeGenerator)
        {
            InvokeCommand(new PropertyChangeCommand<IRangeGenerator, int>(
                rangeGenerator,
                rangeGenerator.N,
                n,
                (m, v) => m.N = v));
        }
        else
        {
            // Not sure if this is possible. Theoretically if this did ever
            // happen, we would just need to propagate a type change event.
            RangeGenerator<double> model = new RangeGenerator<double>(0, n, n / 5.0);
            OnTypeChanged.Invoke(model);
        }
    }

    /// <summary>
    /// Called when the user has changed the step value.
    /// </summary>
    /// <param name="value">The new step value.</param>
    private void OnStepChanged(string value)
    {
        // Check the data type of the value.
        // We need to check what kind of value was provided.
        if (IsDouble(value, out double doubleValue))
            OnValueChanged(doubleValue, (m) => m.Step, (m, v) => m.Step = v);
        else if (IsInt(value, out int integer))
        {
            if (Model is RangeGenerator<double> doubleModel && !IsInt(doubleModel.Start))
                // Start is a double, so we need to convert the step value to a
                // double.
                OnValueChanged((double)integer, (m) => m.Step, (m, v) => m.Step = v);
            else
                // Model is not a double model, or start is an integer and the
                // model may therefore be safely changed to an int model.
                OnValueChanged(integer, (m) => m.Step, (m, v) => m.Step = v);
        }
        else
            throw new InvalidOperationException($"Invalid step value: '{value}'. Must be a number");
    }
}
