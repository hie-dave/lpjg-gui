using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter for a factorial view.
/// </summary>
public class FactorialPresenter : PresenterBase<IFactorialView>, IFactorialPresenter
{
    /// <inheritdoc />
    public Event<IModelChange<FactorialGenerator>> OnChanged => view.OnChanged;

    /// <summary>
    /// Create a new <see cref="FactorialPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view to present the factorial on.</param>
    public FactorialPresenter(IFactorialView view) : base(view)
    {
    }

    /// <inheritdoc />
    public void Populate(FactorialGenerator factorial)
    {
        view.Populate(factorial.FullFactorial);
    }
}
