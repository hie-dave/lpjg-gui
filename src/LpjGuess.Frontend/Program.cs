using Adw;

using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

const string appID = "org.Hie.lpjggui";
const string appName = "LPJ-Guess";

var app = Application.New(appID, Gio.ApplicationFlags.FlagsNone);
GLib.Functions.SetApplicationName(appName);

PangoCairo.Module.Initialize();
Pango.Module.Initialize();
Cairo.Module.Initialize();
GtkSource.Module.Initialize();

app.OnStartup += OnStartup;
app.OnActivate += OnActivated;
app.OnShutdown += OnShutdown;

app.Run(args.Length, args);

void OnStartup(object sender, EventArgs args)
{
	// Perform one-time application initialisation here.
}

async void OnActivated(object sender, EventArgs args)
{
	Application app = (Application)sender;
	MainView window = new MainView(app);
	await window.InitialiseAsync();
	try
	{
		MainPresenter presenter = new MainPresenter(window);
		window.Show();
	}
	catch (Exception error)
	{
		Console.Error.WriteLine(error);
		window.ReportError(error);
		app.Quit();
	}
}

void OnShutdown(object sender, EventArgs args)
{
	// Application has been closed. Any closing logic can go here.
}
