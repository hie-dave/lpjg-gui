using Gio;
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
		Application app = MainView.AppInstance;

		string actionName = name.ToLower().Replace(" ", "-");
		string fullName = $"{domain}.{actionName}";

		menu.Append($"_{name}", fullName);
		var action = SimpleAction.New(actionName, null);
		action.OnActivate += (_, __) => callback();
		if (hotkey != null)
			app.SetAccelsForAction(fullName, new[] { hotkey });
		app.AddAction(action);
	}
}
