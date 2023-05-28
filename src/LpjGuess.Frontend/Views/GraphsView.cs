using OxyPlot.GtkSharp;
using OxyPlot;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to create, modify, and view graphs.
/// </summary>
public class GraphsView : Box, IGraphsView
{
	/// <summary>
	/// Name of the 'add child' item in the stack (name, not title).
	/// </summary>
	private const string addGraphName = "add";

	/// <summary>
	/// Title of the 'add child' item in the stack (title, not name).
	/// </summary>
	private const string addGraphTitle = "Add Graph";

	/// <summary>
	/// Name of the 'visible-child' property of the stack.
	/// </summary>
	private const string visibleChildName = "visible-child";

	/// <summary>
	/// The stack widget.
	/// </summary>
	private readonly Stack stack;

	/// <summary>
	/// The stack sidebar widget.
	/// </summary>
	private readonly StackSidebar sidebar;

	/// <summary>
	/// The widget added to the stack for the "add graph" button.
	/// </summary>
	private readonly Label addGraphWidget;

	/// <summary>
	/// List of plot views.
	/// </summary>
	private List<PlotView> plots;

	/// <summary>
	/// Create a new <see cref="GraphsView"/> instance.
	/// </summary>
	public GraphsView()
	{
		stack = new Stack();
		this.plots = new List<PlotView>();
		OnAddGraph = new Event();

		addGraphWidget = Label.New("");
		addGraphWidget.Visible = false;
		stack.AddTitled(addGraphWidget, addGraphName, addGraphTitle);

		stack.OnNotify += OnStackNotify;

		sidebar = new StackSidebar();
		sidebar.Stack = stack;

		SetOrientation(Orientation.Horizontal);
		Append(sidebar);
		Append(stack);
	}

	/// <inheritdoc />
	public Event OnAddGraph { get; private init; }

	/// <inheritdoc />
	public void Populate(IReadOnlyList<PlotModel> plots)
	{
		EmptyStack();

		foreach (PlotModel model in plots)
			AddPlotModel(model);

		stack.AddTitled(addGraphWidget, addGraphName, addGraphTitle);
	}

	/// <inheritdoc />
	public IReadOnlyList<PlotModel> GetPlots()
	{
		return plots.Select(p => p.Model).ToList();
	}

	/// <inheritdoc />
	public void SelectGraph(int n)
	{
		if (n > plots.Count)
			throw new InvalidOperationException($"Cannot select graph {n}: only {plots.Count} graphs are displayed");
		stack.VisibleChild = plots[n];
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <summary>
	/// Add a graph to the plot. If adding multiple plots, it will be faster to
	/// use <see cref="Populate"/>.
	/// </summary>
	/// <param name="plot"></param>
	public void AddGraph(PlotModel plot)
	{
		stack.Remove(addGraphWidget);
		AddPlotModel(plot);
		stack.AddTitled(addGraphWidget, addGraphName, addGraphTitle);
	}

	/// <summary>
	/// Remove everything from the stack.
	/// </summary>
	private void EmptyStack()
	{
		foreach (PlotView view in this.plots)
		{
			stack.Remove(view);
			view.Dispose();
		}
		this.plots.Clear();
		stack.Remove(addGraphWidget);
	}

	/// <summary>
	/// This is connected to the notify signal of the stack.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	private void OnStackNotify(GObject.Object sender, NotifySignalArgs args)
	{
		try
		{
			if (args.Pspec.GetName() == visibleChildName && stack.VisibleChildName == addGraphName)
				OnAddGraph.Invoke();
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

	/// <summary>
	/// Add the plot model to the stack.
	/// </summary>
	/// <param name="plot">The plot model to be added.</param>
	private void AddPlotModel(PlotModel plot)
	{
		PlotView view = new PlotView();
		if (Gtk.Settings.GetDefault()?.GtkApplicationPreferDarkTheme == true)
		{
			plot.TextColor = plot.PlotAreaBorderColor = OxyColors.White;
		}
		view.Model = plot;
		view.Visible = true;
		view.Hexpand = true;
		view.Vexpand = true;
		plots.Add(view);
		stack.AddTitled(view, $"plot{plots.Count}", plot.Title);
	}
}
