using Gtk;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a concrete top-level parameter.
/// </summary>
[DefaultImplementation]
public class TopLevelParameterView : ViewBase<Grid>, ITopLevelParameterView
{
    /// <summary>
    /// The spacing between child widgets.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

    /// <summary>
    /// The entry widget for the parameter name.
    /// </summary>
    private readonly Entry nameEntry;

    /// <summary>
    /// The label widget for the parameter value.
    /// </summary>
    private readonly Label valueLabel;

    /// <summary>
    /// The entry widget for the parameter value.
    /// </summary>
    private readonly Entry valueEntry;

    /// <inheritdoc />
    public Event<IModelChange<TopLevelParameter>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelParameterView"/> instance.
    /// </summary>
    public TopLevelParameterView() : base(new Grid())
    {
        OnChanged = new Event<IModelChange<TopLevelParameter>>();
        nrow = 0;

        nameEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        valueEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        valueLabel = Label.New("Parameter Value:");

        widget.RowSpacing = widget.ColumnSpacing = spacing;
        widget.Attach(valueLabel, 0, nrow, 1, 1);
        widget.Attach(valueEntry, 1, nrow, 1, 1);

        AddControl("Parameter Name", nameEntry);

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, string value)
    {
        nameEntry.SetText(name);
        valueEntry.SetText(value);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        DisconnectEvents();
        base.Dispose();
    }

    /// <summary>
    /// Add a control to the next row in the grid.
    /// </summary>
    /// <param name="title">The title of the control.</param>
    /// <param name="control">The widget to add.</param>
    protected void AddControl(string title, Widget control)
    {
        // Temporarily remove the value label and entry from the grid.
        widget.Remove(valueLabel);
        widget.Remove(valueEntry);

        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        widget.Attach(label, 0, nrow, 1, 1);
        widget.Attach(control, 1, nrow, 1, 1);
        nrow++;

        // Ensure the value label is always at the bottom of the grid.
        widget.Attach(valueLabel, 0, nrow, 1, 1);
        widget.Attach(valueEntry, 1, nrow, 1, 1);
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    protected virtual void ConnectEvents()
    {
        nameEntry.OnActivate += OnNameChanged;
        valueEntry.OnActivate += OnValueChanged;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    protected virtual void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
        valueEntry.OnActivate -= OnValueChanged;
    }

    /// <summary>
    /// Called when the parameter name has been changed by the user.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<TopLevelParameter, string>(
                p => p.Name,
                (p, name) => p.Name = name,
                nameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the parameter value has been changed by the user.
    /// </summary>
    /// <param name="sender">The entry widget.</param>
    /// <param name="args">The event arguments.</param>
    private void OnValueChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<TopLevelParameter, string>(
                p => p.Value,
                (p, value) => p.Value = value,
                valueEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
