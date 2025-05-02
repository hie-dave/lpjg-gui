using LpjGuess.Classes;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Views.Runners;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Presenters.Runners;

/// <summary>
/// A presenter for a <see cref="LocalRunnerConfiguration"/>.
/// </summary>
public class LocalRunnerConfigurationPresenter : IRunnerPresenter
{
	/// <summary>
	/// The configuration object.
	/// </summary>
	private readonly LocalRunnerConfiguration config;

	/// <summary>
	/// The view object.
	/// </summary>
	private LocalRunnerConfigurationGroupView? view;

	/// <summary>
	/// Is this the default runner?
	/// </summary>
	private readonly bool isDefault;

	/// <summary>
	/// Create a new <see cref="LocalRunnerConfigurationPresenter"/> instance.
	/// </summary>
	/// <param name="config">Runner configuration.</param>
	/// <param name="isDefault">Is this the default runner?</param>
	public LocalRunnerConfigurationPresenter(LocalRunnerConfiguration config, bool isDefault)
	{
		this.isDefault = isDefault;
		this.config = config;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (view != null)
		{
			view.Dispose();
		}
	}

	/// <inheritdoc />
	public IGroupView CreateView()
	{
		view = new LocalRunnerConfigurationGroupView(config.GuessPath);
		view.OnGuessPathChanged.ConnectTo(p => config.GuessPath = p);
		return view;
	}

	/// <inheritdoc />
	public Gtk.Widget GetWidget() => view?.GetWidget() ?? throw new InvalidOperationException($"{config.Name} config is not being edited");

	/// <inheritdoc />
	public IRunnerMetadata GetMetadata()
	{
		return new RunnerMetadata(config.Name, isDefault);
	}
}
