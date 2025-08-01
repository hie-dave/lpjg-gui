using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface to a presenter which controls an experiments view.
/// </summary>
public interface IExperimentsPresenter : IPresenter<IExperimentsView, List<Experiment>>
{
    /// <summary>
    /// Get the experiments as they are currently configured.
    /// </summary>
    List<Experiment> GetExperiments();
}
