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
	/// Create a new <see cref="GraphsView"/> instance.
	/// </summary>
	public GraphsView() : base(new Box())
	{
		OnAddGraph = new Event();
		OnRemoveGraph = new Event<IGraphView>();

		sidebar = new DynamicStackSidebar<IGraphView>(CreateSidebarWidget);
		sidebar.AddText = "Add Graph";
		sidebar.OnAdd.ConnectTo(OnAddGraph);
		sidebar.OnRemove.ConnectTo(OnRemoveGraph);

		widget.SetOrientation(Orientation.Horizontal);
		widget.Append(sidebar);
	}

    private Widget CreateSidebarWidget(IGraphView view)
    {
        Label label = Label.New(view.Model.Title);
		label.Halign = Align.Start;
		label.Name = "GraphsViewSidebarLabel";
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
}
