using System.Runtime.Remoting;
using GObject;
using Gtk;

using ObjectHandle = GObject.Internal.ObjectHandle;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A ColumnView which renders rows with multiple columns containing a single
/// label. The generic type parameter represents the row type. Mapping between
/// row instances and the text rendered in specific columns is done via
/// <see cref="AddColumn"/>.
/// </summary>
/// <typeparam name="TData">The row data type.</typeparam>
public class StringColumnView<TData> : ColumnView where TData : class
{
    /// <summary>
    /// The internal model.
    /// </summary>
	private readonly Gio.ListStore model;

    /// <summary>
    /// The selection object.
    /// </summary>
	private readonly SingleSelection selection;

	/// <summary>
	/// The last column in the view.
	/// </summary>
	private ColumnViewColumn? lastColumn;

	/// <summary>
	/// List of columns in the view.
	/// </summary>
	private readonly List<ColumnViewColumn> columns;

    /// <summary>
    /// Create a new <see cref="StringColumnView{TData}"/> instance.
    /// </summary>
	public StringColumnView()
	{
		model = Gio.ListStore.New(Wrapper.GetGType());
		selection = SingleSelection.New(model);

		Model = selection;
		columns = new List<ColumnViewColumn>();
	}

    /// <summary>
    /// Add a column to the widget.
    /// </summary>
    /// <param name="name">Name of the column.</param>
    /// <param name="renderer">A function which takes a row object as input and returns the value displayed in this column in that row.</param>
	public void AddColumn(string name, Func<TData, string> renderer)
	{
		SignalListItemFactory factory = SignalListItemFactory.New();
		factory.OnSetup += OnSetupLabelColumn;
		factory.OnBind += (_, args) => HandleBind<Label>(args, (l, row) => l.SetText(renderer(row)));
		ColumnViewColumn column = CreateColumn(name, factory);
		AppendColumn(column);
		columns.Add(column);

		if (lastColumn != null)
			lastColumn.Expand = false;
		lastColumn = column;
	}

    /// <summary>
    /// Populate the widget with data.
    /// </summary>
    /// <param name="rows">The data.</param>
	public void Populate(IEnumerable<TData> rows)
	{
		foreach (TData row in rows)
			AddRow(row);
	}

    /// <summary>
    /// Add a single row of data to the widget.
    /// </summary>
    /// <param name="row">The row to be added.</param>
	public void AddRow(TData row)
	{
		model.Append(new Wrapper(row));
	}

    /// <summary>
    /// Remove all rows of data from the model. This does not change the columns
    /// or data-column mappings.
    /// </summary>
	public void Clear()
	{
		model.RemoveAll();
	}

	/// <summary>
	/// Remove all columns from the view.
	/// </summary>
	public void RemoveColumns()
	{
		foreach (ColumnViewColumn column in columns)
		{
			RemoveColumn(column);
			column.Dispose();
		}
		columns.Clear();
		lastColumn = null;
	}

	/// <summary>
	/// Create a column with the specified name.
	/// </summary>
	/// <param name="name">Name of the column.</param>
	/// <param name="factory">Factory object which handles widget creation and data binding.</param>
	/// <returns>The created column object.</returns>
	private ColumnViewColumn CreateColumn(string name, SignalListItemFactory factory)
	{
		ColumnViewColumn column = ColumnViewColumn.New(name, factory);
		column.Expand = true;
		return column;
	}

    /// <summary>
    /// Create the label which will display data for a single cell.
    /// </summary>
    /// <returns>A label widget.</returns>
	private Label CreateLabel()
	{
		return new Label()
		{
			Halign = Align.Start
		};
	}

    /// <summary>
    /// The column setup callback. This creates the widgets used to display data
    /// in a particular column.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
	private void OnSetupLabelColumn(SignalListItemFactory sender, SignalListItemFactory.SetupSignalArgs args)
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			item.SetChild(CreateLabel());
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
	}

    /// <summary>
    /// The bind callback function which binds a particular data row to a
    /// widget which renders that row.
    /// </summary>
    /// <typeparam name="TWidget">The widget type of the column.</typeparam>
    /// <param name="args">Sender object.</param>
    /// <param name="bind">Event data.</param>
	private void HandleBind<TWidget>(SignalListItemFactory.BindSignalArgs args, Action<TWidget, TData> bind)
		where TWidget : Widget
	{
		ListItem item = (ListItem)args.Object;
		Wrapper? wrapper = item.GetItem() as Wrapper;
		TWidget? widget = item.GetChild() as TWidget;
		if (widget != null && wrapper != null)
			bind(widget, wrapper.Data);
	}

    /// <summary>
    /// Wrapper around the data type the user wants to use. This is needed because
    /// the data type needs to be a GObject type.
    /// </summary>
    private class Wrapper : GObject.Object
    {
        /// <summary>
        /// Create a new <see cref="Wrapper"/> instance.
        /// </summary>
        /// <param name="data"></param>
        public Wrapper(TData data) : base(ObjectHandle.For<Wrapper>(Array.Empty<ConstructArgument>()))
        {
            Data = data;
        }

        /// <summary>
        /// A single row of data.
        /// </summary>
        public TData Data { get; private init; }
    }
}
