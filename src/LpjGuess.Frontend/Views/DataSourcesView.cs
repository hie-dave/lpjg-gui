using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which allows the user to manage (add/edit/remove) data sources.
/// </summary>
public class DataSourcesView : IView
{
	/// <summary>
	/// The stack widget which allows for navigation between data sources.
	/// </summary>
	private readonly StackSidebar dataSources;

	/// <summary>
	/// Create a new <see cref="DataSourcesView"/> instance.
	/// </summary>
	public DataSourcesView()
	{
		dataSources = new StackSidebar();
	}

	/// <inheritdoc />
	public Widget GetWidget() => dataSources;

	/// <summary>
	/// Dispose of unmanaged resources.
	/// </summary>
	public void Dispose() => dataSources.Dispose();
}
