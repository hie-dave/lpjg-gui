using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

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

    /// <summary>
    /// The entry widget for the number of values.
    /// </summary>
    private readonly Entry nEntry;

    /// <summary>
    /// The entry widget for the step value.
    /// </summary>
    private readonly Entry stepEntry;

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
        nEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        stepEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
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
        startEntry.SetText(start);
        nEntry.SetText(n.ToString());
        stepEntry.SetText(step);
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
        startEntry.OnActivate += OnStartValueChanged;
        nEntry.OnActivate += OnNValueChanged;
        stepEntry.OnActivate += OnStepValueChanged;
    }

    /// <summary>
    /// Disconnect the event handlers.
    /// </summary>
    private void DisconnectEvents()
    {
        startEntry.OnActivate -= OnStartValueChanged;
        nEntry.OnActivate -= OnNValueChanged;
        stepEntry.OnActivate -= OnStepValueChanged;
    }

    /// <summary>
    /// Called when the start value has been changed by the user.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnStartValueChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnStartChanged.Invoke(sender.GetText());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the number of values has been changed by the user.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnNValueChanged(Entry sender, EventArgs args)
    {
        try
        {
            if (int.TryParse(sender.GetText(), out int n))
                OnNChanged.Invoke(n);
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the step value has been changed by the user.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnStepValueChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnStepChanged.Invoke(sender.GetText());
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
