using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls an experiment view.
/// </summary>
public interface IExperimentPresenter : IPresenter<IExperimentView, Experiment>
{
    /// <summary>
    /// Get the experiment as it is currently configured.
    /// </summary>
    Experiment GetExperiment();

    /// <summary>
    /// Event raised when the experiment is renamed.
    /// </summary>
    Event<string> OnRenamed { get; }
}
