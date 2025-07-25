using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a concrete top-level parameter.
/// </summary>
[RegisterPresenter(typeof(DummyFactor), typeof(IFactorPresenter))]
public class DummyFactorPresenter : PresenterBase<IDummyFactorView, DummyFactor>, IFactorPresenter
{
    /// <inheritdoc />
    public Event<string> OnRenamed { get; }

    /// <inheritdoc />
    public Event OnChanged { get; }

    /// <inheritdoc />
    IFactor IPresenter<IFactor>.Model => Model;

    /// <summary>
    /// Create a new <see cref="DummyFactorPresenter"/> instance.
    /// </summary>
    /// <param name="model">The model to present.</param>
    /// <param name="view">The view to present the model on.</param>
    /// <param name="registry">The command registry to use for command execution.</param>
    public DummyFactorPresenter(
        DummyFactor model,
        IDummyFactorView view,
        ICommandRegistry registry) : base(view, model, registry)
    {
        OnRenamed = new Event<string>();
        OnChanged = new Event();
    }
}
