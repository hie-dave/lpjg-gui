using Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A ColumnView which renders rows with multiple columns containing a single
/// label. The generic type parameter represents the row type. Mapping between
/// row instances and the text rendered in specific columns is done via
/// <see cref="AddColumn"/>.
/// </summary>
/// <typeparam name="TData">The row data type.</typeparam>
public class StringColumnView<TData> : CustomColumnView<TData> where TData : class
{
    /// <summary>
    /// Create a new <see cref="StringColumnView{TData}"/> instance.
    /// </summary>
	public StringColumnView() : base()
	{
	}

    /// <summary>
    /// Add a column to the widget.
    /// </summary>
    /// <param name="name">Name of the column.</param>
    /// <param name="renderer">A function which takes a row object as input and returns the value displayed in this column in that row.</param>
	public void AddColumn(string name, Func<TData, string> renderer)
	{
		AddColumn(name, CreateLabel, (row, label) => label.SetText(renderer(row)));
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
}
