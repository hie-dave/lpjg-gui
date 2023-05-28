using Adw;

using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

const string appName = "org.Hie.lpjggui";

var app = Application.New(appName, Gio.ApplicationFlags.FlagsNone);

PangoCairo.Module.Initialize();
Pango.Module.Initialize();
Cairo.Module.Initialize();

app.OnStartup += OnStartup;
app.OnActivate += OnActivated;
app.OnShutdown += OnShutdown;

app.Run();

void OnStartup(object sender, EventArgs args)
{
	// Perform one-time application initialisation here.
}

void OnActivated(object sender, EventArgs args)
{
	Application app = (Application)sender;
	MainView window = new MainView(app);
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
