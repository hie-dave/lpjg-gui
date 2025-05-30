using Gio;
using GObject;
using Gtk;
using LpjGuess.Frontend.Views;
using System.Reflection;

using Action = System.Action;
using Application = Gtk.Application;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for gtk types.
/// </summary>
public static class GtkExtensions
{
	/// <summary>
	/// Change the column-span property of a child of a grid widget.
	/// </summary>
	/// <param name="grid">The grid widget.</param>
	/// <param name="child">A child of the grid.</param>
	/// <param name="span">The new column-span value.</param>
	public static void SetColumnSpan(this Grid grid, Widget child, int span)
	{
		LayoutManager? layoutManager = grid.LayoutManager;
		if (layoutManager == null)
			throw new InvalidOperationException($"Grid {grid.Name} has no layout manager");

		LayoutChild? layoutChild = layoutManager.GetLayoutChild(child);
		if (layoutChild is GridLayoutChild gridLayout)
			gridLayout.ColumnSpan = span;
		else
			throw new InvalidOperationException($"Child {child.Name} is not a grid layout child");
	}

	/// <summary>
	/// Load style from a resource file embedded in the current assembly.
	/// </summary>
	/// <param name="provider">The style provider.</param>
	/// <param name="resourceName">Name of the embedded resource.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	public static async Task LoadFromEmbeddedResourceAsync(this CssProvider provider, string resourceName
		, CancellationToken cancellationToken = default(CancellationToken))
	{
		string tempFile = Path.GetTempFileName();
		try
		{
			using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new InvalidOperationException($"Resource not found: '{resourceName}'");

				using (Stream writer = System.IO.File.OpenWrite(tempFile))
					await stream.CopyToAsync(writer);
				provider.LoadFromFile(FileHelper.NewForPath(tempFile));
			}
		}
		finally
		{
			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Load style from a resource file embedded in the current assembly.
	/// </summary>
	/// <param name="provider">The style provider.</param>
	/// <param name="resourceName">Name of the embedded resource.</param>
	public static void LoadFromEmbeddedResource(this CssProvider provider, string resourceName)
	{
		provider.LoadFromEmbeddedResourceAsync(resourceName).Wait();
	}

	/// <inheritdoc />
	public static void AddMenuItem(this Menu menu, string domain, string name, Action callback, string? hotkey = null)
	{
		menu.AddMenuItem(domain, name, (_, __) => callback(), hotkey);
	}

	/// <inheritdoc />
	public static void AddMenuItem(this Menu menu, string domain, string name, SignalHandler<SimpleAction, SimpleAction.ActivateSignalArgs> callback, string? hotkey = null)
	{
		Application app = MainView.AppInstance;

		string actionName = name.ToLower().Replace(" ", "-");
		string fullName = $"{domain}.{actionName}";

		menu.Append($"_{name}", fullName);
		var action = SimpleAction.New(actionName, null);
		action.OnActivate += callback;
		if (hotkey != null)
			app.SetAccelsForAction(fullName, new[] { hotkey });
		app.AddAction(action);
		// action.Dispose();
	}
}
