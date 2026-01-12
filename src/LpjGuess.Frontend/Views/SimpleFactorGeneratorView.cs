using Gtk;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to edit a simple factor generator.
/// </summary>
public class SimpleFactorGeneratorView : ViewBase<Box>, ISimpleFactorGeneratorView
{
    /// <summary>
    /// Spacing between the widgets in the box.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The entry for the factor name.
    /// </summary>
    private readonly Entry nameEntry;

    /// <summary>
    /// The container for the factor levels.
    /// </summary>
    private readonly ListBoxStackView container;

    /// <summary>
    /// The grid containing the main controls.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event<IModelChange<SimpleFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddLevel => container.OnAdd;

    /// <inheritdoc />
    public Event<IView> OnRemoveLevel => container.OnRemove;

    /// <summary>
    /// Create a new <see cref="SimpleFactorGeneratorView"/> instance.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    public SimpleFactorGeneratorView(ILoggerFactory factory) : base(new Box())
    {
        OnChanged = new Event<IModelChange<SimpleFactorGenerator>>();

        nameEntry = new Entry();
        nameEntry.Hexpand = true;

        container = new ListBoxStackView(factory.CreateLogger<ListBoxNavigatorView>());
        container.AddText = "Add Level";

        grid = new Grid();
        grid.RowSpacing = spacing;
        grid.ColumnSpacing = spacing;

        AddControl("Name", nameEntry);

        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = spacing;

        widget.Append(grid);
        widget.Append(container.GetWidget());

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(string name, IEnumerable<INamedView> factorLevelViews)
    {
        nameEntry.SetText(name);
        container.Populate(factorLevelViews);
    }

    /// <inheritdoc />
    public void Rename(IView view, string name)
    {
        container.Rename(view, name);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        DisconnectEvents();
        base.Dispose();
    }

    /// <summary>
    /// Add a widget to the grid.
    /// </summary>
    /// <param name="title">The title for the control.</param>
    /// <param name="widget">The widget to add.</param>
    private void AddControl(string title, Widget widget)
    {
        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        grid.Attach(label, 0, nrow, 1, 1);
        grid.Attach(widget, 1, nrow, 1, 1);
        nrow++;
    }

    /// <summary>
    /// Connect gtk event handlers.
    /// </summary>
    private void ConnectEvents()
    {
        nameEntry.OnActivate += OnNameChanged;
    }

    /// <summary>
    /// Disconnect gtk event handlers.
    /// </summary>
    private void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
    }

    /// <summary>
    /// Called when the user has changed the name of the factor.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<SimpleFactorGenerator, string>(
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
