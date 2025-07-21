using LpjGuess.Core.Interfaces;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Interfaces.Presenters;

/// <summary>
/// Base interface for presenters that manage data source editing.
/// </summary>
public interface IDataSourcePresenter : IPresenter
{
    /// <summary>
    /// The data source being edited.
    /// </summary>
    IDataSource DataSource { get; }

    /// <summary>
    /// Called when the data source has been changed.
    /// </summary>
    Event<ICommand> OnDataSourceChanged { get; }

    /// <summary>
    /// Get the view being managed by this presenter.
    /// </summary>
    new IDataSourceView GetView();
}

/// <summary>
/// Interface for presenters that manage data sources.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public interface IDataSourcePresenter<TModel> : IDataSourcePresenter, IPresenter<TModel>
    where TModel : IDataSource
{
    /// <summary>
    /// The data source being edited.
    /// </summary>
    new TModel DataSource { get; }
}
