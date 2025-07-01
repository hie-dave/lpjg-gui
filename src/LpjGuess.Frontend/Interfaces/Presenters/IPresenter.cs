namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// An interface for a presenter.
/// </summary>
public interface IPresenter : IDisposable
{
    /// <summary>
    /// Get the view owned by this presenter.
    /// </summary>
    /// <returns>The view owned by this presenter.</returns>
    IView GetView();
}

/// <summary>
/// An interface to a presenter that manages a model.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IPresenter<TModel> : IPresenter
{
    /// <summary>
    /// Get the model owned by this presenter.
    /// </summary>
    /// <returns>The model owned by this presenter.</returns>
    TModel Model { get; }
}

/// <summary>
/// An interface for a presenter.
/// </summary>
public interface IPresenter<out TView, TModel> : IPresenter<TModel> where TView : IView
{
    /// <summary>
    /// Get the view owned by this presenter.
    /// </summary>
    public new TView GetView();
}
