using LpjGuess.Core.Models;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// Interface to a view which allows the user to edit a single model output data
/// source.
/// </summary>
public interface IModelOutputView : IDataSourceView<ModelOutput>
{
    /// <summary>
    /// Event raised when the user changes the output file type.
    /// </summary>
    Event<OutputFile> OnFileTypeChanged { get; }

    /// <summary>
    /// Populate the view with the given data source.
    /// </summary>
    /// <param name="fileTypes">The file types to display in the file type dropdown.</param>
    /// <param name="columns">The columns to display in the x-axis and y-axis dropdowns.</param>
    /// <param name="fileType">The selected file type.</param>
    /// <param name="xColumn">The selected x-axis column.</param>
    /// <param name="yColumn">The selected y-axis column.</param>
    public void Populate(
        IEnumerable<OutputFile> fileTypes,
        IEnumerable<string> columns,
        OutputFile fileType,
        string xColumn,
        string yColumn);
}
