using Adw;
using Gtk;
using LpjGuess.Runner.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views.Rows;

namespace LpjGuess.Frontend.Views.Runners;

/// <summary>
/// A view which displays settings for a <see cref="LocalRunnerConfiguration"/>.
/// </summary>
internal class LocalRunnerConfigurationGroupView : IGroupView
{
	/// <summary>
	/// Row title/property description.
	/// </summary>
	private const string title = "Path to the LPJ-Guess executable";

	/// <summary>
	/// The preferences group containing the view's UI controls.
	/// </summary>
	private readonly PreferencesGroup group;

	/// <summary>
	/// The row containing the CPU affinity setting.
	/// </summary>
	private readonly SwitchRow cpuAffinityRow;

	/// <summary>
	/// Invoked when the path to the guess executable is changed by the user.
	/// </summary>
	public Event<string> OnGuessPathChanged { get; private init; }

	/// <summary>
	/// Invoked when the CPU affinity setting is changed by the user.
	/// </summary>
	public Event<bool> OnCpuAffinityChanged { get; private init; }

	/// <summary>
	/// The row containing guess executable inputs.
	/// </summary>
	private readonly FileChooserRow executableRow;

	/// <summary>
	/// Create a new <see cref="LocalRunnerConfigurationGroupView"/> instance.
	/// </summary>
	/// <param name="guessPath">Path to the guess executable.</param>
	/// <param name="useCpuAffinity">Whether to use CPU affinity to pin the process to a single CPU.</param>
	public LocalRunnerConfigurationGroupView(string guessPath, bool useCpuAffinity)
	{
		OnGuessPathChanged = new Event<string>();
		OnCpuAffinityChanged = new Event<bool>();

		executableRow = new FileChooserRow(title, guessPath, true);
		executableRow.OnChanged.ConnectTo(OnGuessPathChanged);

		cpuAffinityRow = SwitchRow.New();
		cpuAffinityRow.Active = useCpuAffinity;
		cpuAffinityRow.Title = "CPU affinity";
		cpuAffinityRow.OnNotify += OnCpuAffinityNotify;
		cpuAffinityRow.Subtitle = "True to pin each LPJ-Guess process to a single CPU (ie prevent context-switching of a guess process between CPUs). No effect on MacOS. Recommended: true.";

		group = new PreferencesGroup();
		group.Add(executableRow);
		group.Add(cpuAffinityRow);
	}

    /// <inheritdoc />
    public void Dispose()
	{
		cpuAffinityRow.OnNotify -= OnCpuAffinityNotify;
		OnGuessPathChanged.Dispose();
		OnCpuAffinityChanged.Dispose();
		executableRow.Dispose();
		group.Dispose();
	}

    /// <inheritdoc />
    public PreferencesGroup GetGroup() => group;

	/// <inheritdoc />
	public Widget GetWidget() => GetGroup();

	/// <summary>
	/// Notify signal handler for the CPU affinity row.
	/// </summary>
	/// <param name="sender">The sender object.</param>
	/// <param name="args">The notify signal arguments.</param>
    private void OnCpuAffinityNotify(GObject.Object sender, GObject.Object.NotifySignalArgs args)
    {
        try
		{
			if (args.Pspec.GetName() == "active")
				OnCpuAffinityChanged.Invoke(cpuAffinityRow.Active);
		}
		catch (Exception error)
		{
			MainView.Instance.ReportError(error);
		}
    }
}
