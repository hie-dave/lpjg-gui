using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for a top-level factor generator.
/// </summary>
public class TopLevelFactorGeneratorView : ViewBase<ScrolledWindow>, ITopLevelFactorGeneratorView
{
    /// <summary>
    /// The container for the view.
    /// </summary>
    private readonly Box container;

    /// <summary>
    /// The entry containing the factor name.
    /// </summary>
    private readonly Entry nameEntry;

    /// <summary>
    /// The dropdown for selecting the value generator type.
    /// </summary>
    private readonly EnumDropDownView<ValueGeneratorType> valuesTypeView;

    /// <summary>
    /// The container for the value generator view.
    /// </summary>
    private readonly Box valuesContainer;

    /// <summary>
    /// The grid containing the scalar input controls.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// The view for the value generator.
    /// </summary>
    private IView? valuesView;

    /// <summary>
    /// Number of rows in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event<IModelChange<TopLevelFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event<ValueGeneratorType> OnValuesTypeChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="TopLevelFactorGeneratorView"/> instance.
    /// </summary>
    public TopLevelFactorGeneratorView() : base(new ScrolledWindow())
    {
        nrow = 0;
        OnChanged = new Event<IModelChange<TopLevelFactorGenerator>>();
        OnValuesTypeChanged = new Event<ValueGeneratorType>();

        // Create an input widget for the factor name.
        nameEntry = new Entry();
        nameEntry.Hexpand = true;

        valuesTypeView = new EnumDropDownView<ValueGeneratorType>();
        valuesTypeView.OnSelectionChanged.ConnectTo(OnValuesTypeChanged);

        grid = new Grid();
        grid.RowSpacing = grid.ColumnSpacing = 6;
        AddControl("Parameter", nameEntry);
        AddControl("Values", valuesTypeView.GetWidget());

        valuesContainer = new Box();
        valuesView = null;

        // Pack child widgets into the container.
        container = Box.New(Orientation.Vertical, 6);
        container.Append(grid);
        container.Append(valuesContainer);

        // Pack the container into the main widget.
        widget.Child = container;
        widget.Vexpand = true;

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, IView valueGeneratorView)
    {
        // Populate the name entry.
        nameEntry.SetText(name);

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
        base.Dispose();
    }

    /// <summary>
    /// Add a widget to the grid containing the scalar input controls.
    /// </summary>
    protected void AddControl(string name, Widget widget)
    {
        Label label = Label.New($"{name}:");
        grid.Attach(label, 0, nrow, 1, 1);
        grid.Attach(widget, 1, nrow, 1, 1);
        nrow++;
    }

    /// <summary>
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
        nameEntry.OnActivate += OnNameChanged;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
    }

    /// <summary>
    /// Called when the name entry is activated.
    /// </summary>
    /// <param name="sender">The name entry.</param>
    /// <param name="args">The event arguments.</param>
    private void OnNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<TopLevelFactorGenerator, string>(
                f => f.Name,
                (f, name) => f.Name = name,
                nameEntry.GetText()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
