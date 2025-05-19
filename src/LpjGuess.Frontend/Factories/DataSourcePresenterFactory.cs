using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Interfaces.Factories;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Presenters;
using LpjGuess.Frontend.Views;

namespace LpjGuess.Frontend.Factories;

/// <summary>
/// Factory for creating series presenters based on series type.
/// </summary>
public class DataSourcePresenterFactory : IDataSourcePresenterFactory
{
    /// <summary>
    /// Instruction files in the current workspace.
    /// </summary>
    private readonly IEnumerable<string> instructionFiles;

    /// <summary>
    /// Create a new <see cref="DataSourcePresenterFactory"/> instance.
    /// </summary>
    public DataSourcePresenterFactory(IEnumerable<string> instructionFiles)
    {
        this.instructionFiles = instructionFiles;
    }

    /// <summary>
    /// Creates an appropriate presenter for the given series.
    /// </summary>
    /// <param name="dataSource">The data source to create a presenter for.</param>
    /// <returns>A presenter compatible with the data source type.</returns>
    public IDataSourcePresenter CreatePresenter(IDataSource dataSource)
    {
        return dataSource switch
        {
            ModelOutput modelOutput => CreateModelOutputPresenter(modelOutput),
            _ => throw new ArgumentException($"No presenter available for data source type {dataSource.GetType().Name}")
        };
    }

    /// <summary>
    /// Create a presenter for a model output data source.
    /// </summary>
    /// <param name="modelOutput">The model output data source.</param>
    /// <returns>A presenter for the model output data source.</returns>
    private IDataSourcePresenter CreateModelOutputPresenter(ModelOutput modelOutput)
    {
        IModelOutputView view = new ModelOutputView();
        return new ModelOutputPresenter(view, modelOutput, instructionFiles);
    }
}
