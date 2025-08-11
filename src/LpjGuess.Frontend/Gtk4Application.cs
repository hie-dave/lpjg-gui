using Adw;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<Gtk4Application> logger;

    /// <summary>
    /// Create a new <see cref="Gtk4Application"/> instance.
    /// </summary>
    /// <param name="factory">The presenter factory.</param>
    /// <param name="logger">The logger.</param>
    public Gtk4Application(
        IPresenterFactory factory,
        ILogger<Gtk4Application> logger)
    {
        this.factory = factory;
        this.logger = logger;

        app = Application.New(appID, Gio.ApplicationFlags.HandlesOpen);
        GLib.Functions.SetApplicationName(appName);
        app.OnOpen += OnOpen;
        app.OnStartup += OnStartup;
        app.OnActivate += OnActivated;
        app.OnShutdown += OnShutdown;
    }

    private void OnOpen(Gio.Application sender, Gio.Application.OpenSignalArgs args)
    {
        logger.LogInformation("Open {n} files: [{args}]",
            args.NFiles,
            string.Join(", ", args.Files.Select(f => f.GetPath())));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        app.Dispose();
    }

    /// <inheritdoc />
    public void Run(string[] args)
    {
        logger.LogInformation("Passing {n} arguments Gtk: [{args}]",
            args.Length,
            string.Join(", ", args));
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
            logger.LogError(error, "Failed to activate application");
            MainView.Instance.ReportError(error);
            app.Quit();
        }
    }

    private void OnShutdown(object sender, EventArgs args)
    {
        // Application has been closed. Any closing logic can go here.
    }
}
