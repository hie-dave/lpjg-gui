using System.Runtime.Remoting;
using GObject;
using Gtk;
using LpjGuess.Frontend.Interfaces;
using ObjectHandle = GObject.Internal.ObjectHandle;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A ColumnView which renders rows with multiple columns containing a single
/// label. The generic type parameter represents the row type. Mapping between
/// row instances and the text rendered in specific columns is done via
/// <see cref="AddColumn"/>.
/// </summary>
/// <typeparam name="TData">The row data type.</typeparam>
public class CustomColumnView<TData> : ColumnView where TData : class
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
	public CustomColumnView()
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
    /// <param name="widgetFactory">A function which creates the widget to be used to display data in this column.</param>
    /// <param name="renderer">A function which binds a row object to a widget.</param>
	public void AddColumn<TWidget>(
        string name,
        Func<TWidget> widgetFactory,
        Action<TData, TWidget> renderer) where TWidget : Widget
	{
		SignalListItemFactory factory = SignalListItemFactory.New();
		factory.OnSetup += (_, args) => HandleSetup(args, widgetFactory);
		factory.OnBind += (_, args) => HandleBind<TWidget>(args, renderer);
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
		RemoveRows();
        RemoveColumns();
	}

    /// <summary>
    /// Remove all rows of data.
    /// </summary>
    protected void RemoveRows()
    {
        model.RemoveAll();
    }

	/// <summary>
	/// Remove all columns from the view.
	/// </summary>
	private void RemoveColumns()
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
    /// The column setup callback. This creates the widgets used to display data
    /// in a particular column.
    /// </summary>
    /// <param name="args">Event data.</param>
    /// <param name="widgetFactory">Function which creates the widget to be used to display data in a particular column.</param>
	private static void HandleSetup<TWidget>(SignalListItemFactory.SetupSignalArgs args, Func<TWidget> widgetFactory) where TWidget : Widget
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			item.SetChild(widgetFactory());
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
	/// <param name="bind">Function which binds a data row to a widget.</param>
	private static void HandleBind<TWidget>(SignalListItemFactory.BindSignalArgs args, Action<TData, TWidget> bind)
		where TWidget : Widget
	{
		try
		{
			ListItem item = (ListItem)args.Object;
			Wrapper? wrapper = item.GetItem() as Wrapper;
			TWidget? widget = item.GetChild() as TWidget;
			if (widget != null && wrapper != null)
				bind(wrapper.Data, widget);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
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
