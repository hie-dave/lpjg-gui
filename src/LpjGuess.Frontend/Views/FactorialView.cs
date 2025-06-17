using Gtk;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a factorial.
/// </summary>
public class FactorialView : ViewBase<Box>, IFactorialView
{
    /// <summary>
    /// Widget spacing.
    /// </summary>
    private readonly int spacing = 6;

    /// <summary>
    /// The grid which displays the factorial properties.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// The switch which controls whether to generate a full factorial.
    /// </summary>
    private readonly Switch fullFactorialSwitch;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

    /// <inheritdoc />
    public Event<IModelChange<FactorialGenerator>> OnChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="FactorialView"/> instance.
    /// </summary>
    public FactorialView() : base(new Box())
    {
        nrow = 0;
        OnChanged = new Event<IModelChange<FactorialGenerator>>();

        // Configure container.
        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = spacing;

        // Initialise and configure child widgets.
        grid = new Grid();
        grid.RowSpacing = spacing;
        grid.ColumnSpacing = spacing;

        fullFactorialSwitch = new Switch();

        // Pack controls into the grid.
        AddControl("Full Factorial", fullFactorialSwitch);

        // Pack child widgets into the container.
        widget.Append(grid);

        // Connect event handlers.
        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(bool fullFactorial)
    {
        fullFactorialSwitch.SetActive(fullFactorial);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        base.Dispose();
    }

    /// <summary>
    /// Add a control to the grid.
    /// </summary>
    /// <param name="title">The title of the control.</param>
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
    /// Connect all events.
    /// </summary>
    private void ConnectEvents()
    {
        fullFactorialSwitch.OnActivate += OnFullFactorialChanged;
    }

    /// <summary>
    /// Disconnect all events.
    /// </summary>
    private void DisconnectEvents()
    {
        fullFactorialSwitch.OnActivate -= OnFullFactorialChanged;
    }

    /// <summary>
    /// Called when the full factorial switch is toggled by the user.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnFullFactorialChanged(object sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<FactorialGenerator, bool>(
                f => f.FullFactorial,
                (f, fullFactorial) => f.FullFactorial = fullFactorial,
                fullFactorialSwitch.GetActive()
            ));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
