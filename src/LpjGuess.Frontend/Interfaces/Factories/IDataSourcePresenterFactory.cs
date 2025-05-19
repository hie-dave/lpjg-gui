using LpjGuess.Core.Interfaces;
using LpjGuess.Frontend.Interfaces.Presenters;

namespace LpjGuess.Frontend.Interfaces.Factories;

/// <summary>
/// Factory for creating data source presenters based on data source type.
/// </summary>
public interface IDataSourcePresenterFactory
{
    /// <summary>
    /// Creates an appropriate presenter for the given data source.
    /// </summary>
    /// <param name="dataSource">The data source to create a presenter for.</param>
    /// <returns>A presenter compatible with the data source type.</returns>
    IDataSourcePresenter CreatePresenter(IDataSource dataSource);
}
