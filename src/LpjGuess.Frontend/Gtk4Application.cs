using Adw;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend;

/// <summary>
/// Gtk4 implementation of <see cref="IApplication"/>.
/// </summary>
public class Gtk4Application : IApplication
{
    static Gtk4Application()
    {
        Adw.Module.Initialize();
        PangoCairo.Module.Initialize();
        Pango.Module.Initialize();
        Cairo.Module.Initialize();
        GtkSource.Module.Initialize();
    }

    /// <summary>
    /// GNOME Application ID.
    /// </summary>
    const string appID = "org.Hie.lpjggui";

    /// <summary>
    /// GNOME Application Name.
    /// </summary>
    const string appName = "LPJ-Guess";

    /// <summary>
    /// The application object.
    /// </summary>
    private readonly Adw.Application app;

    /// <summary>
    /// The presenter factory.
    /// </summary>
    private readonly IPresenterFactory factory;

    /// <inheritdoc />
    public Gtk4Application(IPresenterFactory factory)
    {
        this.factory = factory;
        app = Application.New(appID, Gio.ApplicationFlags.FlagsNone);
        GLib.Functions.SetApplicationName(appName);
        app.OnStartup += OnStartup;
        app.OnActivate += OnActivated;
        app.OnShutdown += OnShutdown;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        app.Dispose();
    }

    /// <inheritdoc />
    public void Run(string[] args)
    {
        app.Run(args.Length, args);
    }

    private void OnStartup(object sender, EventArgs args)
    {
        // Perform one-time application initialisation here.
    }

    private async void OnActivated(object sender, EventArgs args)
    {
        try
        {
            IMainPresenter presenter = factory.CreatePresenter<IMainPresenter>();
            await presenter.InitialiseAsync();
            presenter.Show();
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            MainView.Instance.ReportError(error);
            app.Quit();
        }
    }

    private void OnShutdown(object sender, EventArgs args)
    {
        // Application has been closed. Any closing logic can go here.
    }
}
