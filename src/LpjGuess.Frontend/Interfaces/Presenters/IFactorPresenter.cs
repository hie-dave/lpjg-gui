using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter which manages a factor.
/// </summary>
public interface IFactorPresenter : IPresenter<IFactor>
{
    /// <summary>
    /// Event which is raised when the name of the factor has been changed by the user.
    /// </summary>
    Event<string> OnRenamed { get; }

    /// <summary>
    /// Event which is raised when the factor has been changed by the user.
    /// </summary>
    Event OnChanged { get; }

    /// <summary>
    /// Set optional parameter target suggestions for this editor.
    /// </summary>
    void SetTargetSuggestions(IEnumerable<ParameterTarget> targets);
}
