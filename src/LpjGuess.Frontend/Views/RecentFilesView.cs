using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view displayed in the main window when no file is selected.
/// </summary>
public class RecentFilesView : Box, IRecentFilesView
{
	/// <summary>
	/// Container for the list of recent files.
	/// </summary>
	private readonly ListBox container;

	/// <summary>
	/// Widgets inside the container.
	/// </summary>
	private readonly List<Widget> widgets;

	/// <summary>
	/// Create a new <see cref="RecentFilesView"/> instance.
	/// </summary>
	public RecentFilesView() : base()
	{
		widgets = new List<Widget>();
		OnClick = new Event<string>();

		Hexpand = true;
		Vexpand = true;
		SetOrientation(Orientation.Vertical);

		container = new ListBox();
		container.Valign = Align.Center;
		container.Halign = Align.Center;
		container.ActivateOnSingleClick = true;
		container.OnRowActivated += OnRowActivated;
		container.ShowSeparators = true;
		// container.AddCssClass("view");
		// container.AddCssClass("frame");
		// container.AddCssClass("rich-list");
		container.AddCssClass("boxed-list");

		Label title = Label.New("<big><b>Recent Workspaces</b></big>");
		title.MarginTop = 8;
		title.MarginBottom = 8;
		title.UseMarkup = true;
		Append(title);

		Append(container);
	}

    /// <inheritdoc />	
    public Event<string> OnClick { get; private init; }

    /// <inheritdoc />
    public Widget GetWidget() => this;

	/// <inheritdoc />
    public void Populate(IEnumerable<string> files)
    {
		Clear();
		foreach (string file in files)
		{
			Box box = new Box();
			box.SetOrientation(Orientation.Vertical);
			box.Spacing = 5;
			const int margin = 5;
			box.MarginBottom = margin;
			box.MarginTop = margin;
			box.MarginStart = 2 * margin;
			box.MarginEnd = 2 * margin;

			string basename = Path.GetFileName(file);
			Label title = Label.New($"<b>{basename}</b>");
			title.UseMarkup = true;
			Label subtitle = Label.New(file);
			subtitle.AddCssClass("dim-label");
			box.Append(title);
			box.Append(subtitle);

			// The name of the widgets added to the ListBox must be the file
			// path. This is then later used in the activated callback.
			ListBoxRow row = new ListBoxRow();
			row.Name = file;
			row.Child = box;

			container.Append(row);
			widgets.Add(row);
		}
    }

	/// <summary>
	/// Disconnect events and dispose of native resources.
	/// </summary>
    public override void Dispose()
    {
		OnClick.Dispose();
		container.OnRowActivated -= OnRowActivated;
        base.Dispose();
    }

	/// <summary>
	/// Remove all widgets from the container.
	/// </summary>
	private void Clear()
	{
        foreach (Widget widget in widgets)
			container.Remove(widget);
	}

	/// <summary>
	/// Called when a row is activated.
	/// </summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="args">Event data.</param>
	/// <exception cref="InvalidOperationException">Thrown if the activated row has a null name.</exception>
    private void OnRowActivated(ListBox sender, ListBox.RowActivatedSignalArgs args)
    {
        try
		{
			string? file = args.Row.Name;
			if (file == null)
				throw new InvalidOperationException($"Activated row name is null - this should never happen");
			OnClick.Invoke(file);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }
}
