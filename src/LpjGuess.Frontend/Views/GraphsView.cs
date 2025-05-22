using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Extensions;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to create, modify, and view graphs.
/// </summary>
public class GraphsView : ViewBase<Box>, IGraphsView
{
	/// <summary>
	/// The stack sidebar widget.
	/// </summary>
	private readonly DynamicStackSidebar<IGraphView> sidebar;

	/// <summary>
	/// The graph presenter associated with each graph view.
	/// </summary>
	private readonly Dictionary<IGraphView, Label> sidebarWidgets;

	/// <summary>
	/// Create a new <see cref="GraphsView"/> instance.
	/// </summary>
	public GraphsView() : base(new Box())
	{
		OnAddGraph = new Event();
		OnRemoveGraph = new Event<IGraphView>();
		sidebarWidgets = new Dictionary<IGraphView, Label>();

		sidebar = new DynamicStackSidebar<IGraphView>(CreateSidebarWidget);
		sidebar.AddText = "Add Graph";
		sidebar.OnAdd.ConnectTo(OnAddGraph);
		sidebar.OnRemove.ConnectTo(OnRemoveGraph);

		widget.SetOrientation(Orientation.Horizontal);
		widget.Append(sidebar);
	}

    private Widget CreateSidebarWidget(IGraphView view)
    {
        Label label = Label.New(SanitiseGraphTitle(view.Model.Title));
		label.Halign = Align.Start;
		label.Name = "GraphsViewSidebarLabel";

		sidebarWidgets[view] = label;

		return label;
    }

    /// <inheritdoc />
    public Event OnAddGraph { get; private init; }

    /// <inheritdoc />
	public Event<IGraphView> OnRemoveGraph { get; private init; }

	/// <inheritdoc />
	public void Populate(IEnumerable<IGraphView> plots)
	{
		IEnumerable<(IGraphView, Widget)> views = plots
			.Select(view => (view, view.GetWidget()));
		sidebar.Populate(views);
	}

    /// <inheritdoc />
	public void Rename(IGraphView view, string title)
	{
		sidebarWidgets[view].SetText(SanitiseGraphTitle(title));
	}

	/// <summary>
	/// Sanitise a graph title for display in the sidebar.
	/// </summary>
	/// <param name="title">The title to sanitise.</param>
	/// <returns>The sanitised title.</returns>
	private static string SanitiseGraphTitle(string title)
	{
		if (string.IsNullOrWhiteSpace(title))
			return "Untitled Graph";
		return title;
	}
}
