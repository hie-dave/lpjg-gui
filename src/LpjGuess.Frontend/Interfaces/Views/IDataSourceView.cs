using LpjGuess.Core.Interfaces;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which allows the user to edit a single data source.
/// </summary>
public interface IDataSourceView : IView
{
    /// <summary>
    /// Gets the views needed to configure this data source type, which are
    /// suitable for adding to a grid.
    /// </summary>
    /// <returns>An enumerable of named views for configuring the data
    /// source.</returns>
    IEnumerable<INamedView> GetGridConfigViews();

    /// <summary>
    /// Gets the views needed to configure this data source type, which are
    /// not suitable for adding to a grid.
    /// </summary>
    /// <returns>An enumerable of named views for configuring the data
    /// source.</returns>
    IEnumerable<INamedView> GetExtraConfigViews();
}

/// <summary>
/// Interface to a view which allows the user to edit a single data source of a
/// specific type.
/// </summary>
public interface IDataSourceView<T> : IDataSourceView where T : IDataSource
{
    /// <summary>
    /// Called when the user wants to edit the data source.
    /// The event parameter is the action to perform on the data source.
    /// </summary>
    Event<IModelChange<T>> OnEditDataSource { get; }
}
