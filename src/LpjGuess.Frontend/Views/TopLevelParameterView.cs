using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

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
    /// The entry widget for the parameter name.
    /// </summary>
    protected readonly SuggestionEntryView nameEntry;

    /// <summary>
    /// The entry widget for the parameter value.
    /// </summary>
    protected readonly Entry valueEntry;
    private readonly EntryCommitter valueCommitter;
    private readonly List<Label> controlLabels;

    /// <inheritdoc />
    public Event<IModelChange<TopLevelParameter>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelParameterView"/> instance.
    /// </summary>
    public TopLevelParameterView() : base(new Grid())
    {
        OnChanged = new Event<IModelChange<TopLevelParameter>>();
        controlLabels = [];

        nameEntry = new SuggestionEntryView("e.g. npatch", showHint: true);
        valueEntry = new Entry() { Halign = Align.Fill, Hexpand = true };
        valueEntry.PlaceholderText = "Value";
        nameEntry.OnCommitted.ConnectTo(OnNameChanged);
        valueCommitter = new EntryCommitter(valueEntry, OnValueChanged);

        widget.RowSpacing = widget.ColumnSpacing = spacing;
        SetControls(
            ("Parameter Name", nameEntry.GetWidget()),
            ("Parameter Value", valueEntry));

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, string value)
    {
        nameEntry.SetText(name);
        valueCommitter.SetText(value);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        DisconnectEvents();
        nameEntry.Dispose();
        valueCommitter.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public virtual void SetTargetSuggestions(IEnumerable<ParameterTarget> targets)
    {
        nameEntry.SetSuggestions(targets
            .Where(target => target.BlockType is null)
            .Select(target => target.ParameterName));
    }

    /// <summary>
    /// Set the controls and their order in the grid.
    /// </summary>
    protected void SetControls(params (string Title, Widget Control)[] controls)
    {
        Widget? child;
        while ((child = widget.GetFirstChild()) != null)
            widget.Remove(child);
        controlLabels.ForEach(label => label.Dispose());
        controlLabels.Clear();

        for (int row = 0; row < controls.Length; row++)
        {
            (string title, Widget control) = controls[row];
            Label label = Label.New($"{title}:");
            label.Halign = Align.Start;
            controlLabels.Add(label);
            widget.Attach(label, 0, row, 1, 1);
            widget.Attach(control, 1, row, 1, 1);
        }
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    protected virtual void ConnectEvents()
    {
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    protected virtual void DisconnectEvents()
    {
    }

    /// <summary>
    /// Commit a changed parameter name.
    /// </summary>
    /// <param name="value">The new parameter name.</param>
    private void OnNameChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<TopLevelParameter, string>(
            parameter => parameter.Name,
            (parameter, name) => parameter.Name = name,
            value));
    }

    /// <summary>
    /// Commit a changed parameter value.
    /// </summary>
    /// <param name="value">The new parameter value.</param>
    private void OnValueChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<TopLevelParameter, string>(
            parameter => parameter.Value,
            (parameter, newValue) => parameter.Value = newValue,
            value));
    }
}
