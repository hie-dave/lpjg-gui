using Gtk;
using System.Globalization;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to edit a range values generator.
/// </summary>
public class RangeValuesView : ViewBase<Box>, IRangeValuesView
{
    /// <summary>
    /// The spacing between child widgets.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The grid containing the controls.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// The entry widget for the start value.
    /// </summary>
    private readonly Entry startEntry;
    private readonly EntryCommitter startCommitter;

    /// <summary>
    /// The entry widget for the number of values.
    /// </summary>
    private readonly Entry nEntry;
    private readonly EntryCommitter nCommitter;

    /// <summary>
    /// The entry widget for the step value.
    /// </summary>
    private readonly Entry stepEntry;
    private readonly EntryCommitter stepCommitter;

    /// <summary>
    /// The label widget displaying the values which would be generated.
    /// </summary>
    private readonly Label valuesHint;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event<string> OnStartChanged { get; private init; }

    /// <inheritdoc />
    public Event<int> OnNChanged { get; private init; }

    /// <inheritdoc />
    public Event<string> OnStepChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="RangeValuesView"/> instance.
    /// </summary>
    public RangeValuesView() : base(new Box())
    {
        OnStartChanged = new Event<string>();
        OnNChanged = new Event<int>();
        OnStepChanged = new Event<string>();

        startEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        nEntry = new Entry()
        {
            Halign = Align.Fill,
            Hexpand = true,
            InputPurpose = InputPurpose.Digits
        };
        stepEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        startCommitter = new EntryCommitter(startEntry, OnStartValueChanged);
        nCommitter = new EntryCommitter(
            nEntry,
            OnNValueChanged,
            ValidateNumberOfValues);
        stepCommitter = new EntryCommitter(stepEntry, OnStepValueChanged);
        valuesHint = new Label();
        valuesHint.Halign = Align.Start;
        valuesHint.Ellipsize = Pango.EllipsizeMode.Middle;

        grid = new Grid();
        grid.RowSpacing = grid.ColumnSpacing = spacing;
        AddControl("Start Value", startEntry);
        AddControl("Number of Values", nEntry);
        AddControl("Step Size", stepEntry);

        widget.SetOrientation(Orientation.Vertical);
        widget.SetSpacing(spacing);
        widget.Append(grid);
        widget.Append(valuesHint);

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string start, int n, string step, IEnumerable<string> values)
    {
        startCommitter.SetText(start);
        nCommitter.SetText(n.ToString());
        stepCommitter.SetText(step);
        valuesHint.SetText($"Values: {string.Join(", ", values)}");
    }

    /// <inheritdoc />
    public void SetMoreValuesIndicator(bool moreValues)
    {
        if (moreValues)
            // Place the ellipsis at the end of the string.
            valuesHint.Ellipsize = Pango.EllipsizeMode.End;
        else
            // Place the ellipsis in the middle of the string. In this case, the
            // ellipsis will only be displayed if the number of values exceeds
            // the available space for the label.
            valuesHint.Ellipsize = Pango.EllipsizeMode.Middle;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnStartChanged.Dispose();
        OnNChanged.Dispose();
        OnStepChanged.Dispose();
        DisconnectEvents();
        startCommitter.Dispose();
        nCommitter.Dispose();
        stepCommitter.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Add a control to the grid.
    /// </summary>
    /// <param name="title">The title for the control.</param>
    /// <param name="control">The widget to add.</param>
    private void AddControl(string title, Widget control)
    {
        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        grid.Attach(label, 0, nrow, 1, 1);
        grid.Attach(control, 1, nrow, 1, 1);
        nrow++;
    }

    /// <summary>
    /// Connect the event handlers.
    /// </summary>
    private void ConnectEvents()
    {
    }

    /// <summary>
    /// Disconnect the event handlers.
    /// </summary>
    private void DisconnectEvents()
    {
    }

    /// <summary>
    /// Commit a changed start value.
    /// </summary>
    /// <param name="value">The new start value.</param>
    private void OnStartValueChanged(string value)
    {
        OnStartChanged.Invoke(value);
    }

    /// <summary>
    /// Commit a changed number of values.
    /// </summary>
    /// <param name="value">The new count.</param>
    private void OnNValueChanged(string value)
    {
        int n = int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        OnNChanged.Invoke(n);
    }

    /// <summary>
    /// Validate the number of values without throwing from a GTK callback.
    /// </summary>
    private static string? ValidateNumberOfValues(string value)
    {
        if (!int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int n) ||
            n <= 0)
        {
            return "Number of values must be a positive whole number.";
        }

        return null;
    }

    /// <summary>
    /// Commit a changed step value.
    /// </summary>
    /// <param name="value">The new step value.</param>
    private void OnStepValueChanged(string value)
    {
        OnStepChanged.Invoke(value);
    }
}
