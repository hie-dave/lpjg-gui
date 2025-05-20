using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Wrapper around a view widget which provides a name for the view.
/// </summary>
public class NamedView : INamedView
{
    /// <inheritdoc/>
    public IView View { get; private init; }

    /// <inheritdoc/>
    public string Name { get; private init; }

    /// <summary>
    /// Create a new <see cref="NamedView"/> instance.
    /// </summary>
    /// <param name="view">The view object.</param>
    /// <param name="name">The name of the view.</param>
    public NamedView(IView view, string name)
    {
        View = view;
        Name = name;
    }
}
