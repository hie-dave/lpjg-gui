namespace LpjGuess.Frontend.Interfaces.Factories;

/// <summary>
/// A factory for creating views.
/// </summary>
public interface IViewFactory
{
    /// <summary>
    /// Create a new view for the specified model.
    /// </summary>
    /// <returns>The view.</returns>
    T CreateView<T>() where T : IView;
}
