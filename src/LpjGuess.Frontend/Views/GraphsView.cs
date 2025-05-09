using OxyPlot.GtkSharp;
using OxyPlot;
using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using LpjGuess.Frontend.Extensions;

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
	/// The stack sidebar widget.
	/// </summary>
	private readonly DynamicStackSidebar<PlotModel> sidebar;

	/// <summary>
	/// List of plot views.
	/// </summary>
	private List<PlotView> plots;

	/// <summary>
	/// Create a new <see cref="GraphsView"/> instance.
	/// </summary>
	public GraphsView()
	{
		this.plots = new List<PlotView>();
		OnAddGraph = new Event();
		OnRemoveGraph = new Event<PlotModel>();

		sidebar = new DynamicStackSidebar<PlotModel>(CreateSidebarWidget);
		sidebar.AddText = "Add Graph";
		sidebar.OnAdd.ConnectTo(OnAddGraph);

		SetOrientation(Orientation.Horizontal);
		Append(sidebar);
	}

    private Widget CreateSidebarWidget(PlotModel model)
    {
        Label label = Label.New(model.Title);
		label.Halign = Align.Start;
		return label;
    }

    /// <inheritdoc />
    public Event OnAddGraph { get; private init; }

    /// <inheritdoc />
	public Event<PlotModel> OnRemoveGraph { get; private init; }

	/// <inheritdoc />
	public void Populate(IEnumerable<PlotModel> plots)
	{
		this.plots.Clear();

		IEnumerable<(PlotModel, Widget)> views = plots
			.Select(model => (model, CreatePlotView(model) as Widget));
		sidebar.Populate(views);
	}

	/// <inheritdoc />
	public IEnumerable<PlotModel> GetPlots()
	{
		return plots.Select(p => p.Model).ToList();
	}

	/// <inheritdoc />
	public Widget GetWidget() => this;

	/// <summary>
	/// Add the plot model to the stack.
	/// </summary>
	/// <param name="plot">The plot model to be added.</param>
	private PlotView CreatePlotView(PlotModel plot)
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
		return view;
	}
}
