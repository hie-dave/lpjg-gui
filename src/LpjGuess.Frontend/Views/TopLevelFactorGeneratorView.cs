using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a top-level factor generator.
/// </summary>
[DefaultImplementation]
public class TopLevelFactorGeneratorView : ViewBase<Box>, ITopLevelFactorGeneratorView
{
    /// <summary>
    /// The entry containing the factor name.
    /// </summary>
    protected readonly SuggestionEntryView nameEntry;

    /// <summary>
    /// The dropdown for selecting the value generator type.
    /// </summary>
    protected readonly EnumDropDownView<ValueGeneratorType> valuesTypeView;

    /// <summary>
    /// The container for the value generator view.
    /// </summary>
    private readonly Box valuesContainer;

    /// <summary>
    /// The grid containing the scalar input controls.
    /// </summary>
    private readonly Grid grid;
    private readonly List<Label> controlLabels;

    /// <summary>
    /// The view for the value generator.
    /// </summary>
    private IView? valuesView;

    /// <inheritdoc />
    public Event<IModelChange<TopLevelFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event<ValueGeneratorType> OnValuesTypeChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGeneratorView"/> instance.
    /// </summary>
    public TopLevelFactorGeneratorView() : base(Box.New(Orientation.Vertical, 6))
    {
        OnChanged = new Event<IModelChange<TopLevelFactorGenerator>>();
        OnValuesTypeChanged = new Event<ValueGeneratorType>();
        controlLabels = [];

        // Create an input widget for the factor name.
        nameEntry = new SuggestionEntryView("e.g. npatch", showHint: true);
        nameEntry.OnCommitted.ConnectTo(OnNameChanged);

        valuesTypeView = new EnumDropDownView<ValueGeneratorType>();
        valuesTypeView.OnSelectionChanged.ConnectTo(OnValuesTypeChanged);

        grid = new Grid();
        grid.RowSpacing = grid.ColumnSpacing = 6;
        SetControls(
            ("Parameter Name", nameEntry.GetWidget()),
            ("Values", valuesTypeView.GetWidget()));

        valuesContainer = new Box();
        valuesView = null;

        // Pack child widgets into the container.
        widget.Append(grid);
        widget.Append(valuesContainer);

        // Pack the container into the main widget.
        widget.Vexpand = true;

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, ValueGeneratorType valueGeneratorType, IView valueGeneratorView)
    {
        // Populate the name entry.
        nameEntry.SetText(name);

        valuesTypeView.Select(valueGeneratorType);

        // ListBoxes don't own their contents.
        if (valuesView != null)
            valuesContainer.Remove(valuesView.GetWidget());

        valuesView = valueGeneratorView;
        valuesContainer.Append(valuesView.GetWidget());
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        nameEntry.Dispose();
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
    /// Set the scalar controls and their order in the grid.
    /// </summary>
    protected void SetControls(params (string Title, Widget Control)[] controls)
    {
        Widget? child;
        while ((child = grid.GetFirstChild()) != null)
            grid.Remove(child);
        controlLabels.ForEach(label => label.Dispose());
        controlLabels.Clear();

        for (int row = 0; row < controls.Length; row++)
        {
            (string title, Widget control) = controls[row];
            Label label = Label.New($"{title}:");
            label.Halign = Align.Start;
            controlLabels.Add(label);
            grid.Attach(label, 0, row, 1, 1);
            grid.Attach(control, 1, row, 1, 1);
        }
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
    }

    /// <summary>
    /// Commit a changed parameter name.
    /// </summary>
    /// <param name="value">The new parameter name.</param>
    private void OnNameChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<TopLevelFactorGenerator, string>(
            factor => factor.Name,
            (factor, name) => factor.Name = name,
            value));
    }
}
