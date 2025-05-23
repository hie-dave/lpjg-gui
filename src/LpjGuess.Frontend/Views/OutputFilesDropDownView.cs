using LpjGuess.Core.Models;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A drop-down view which displays a list of output files, grouped by quantity
/// type (e.g. "LAI").
/// </summary>
public class OutputFilesDropDownView : GroupedDropDownView<OutputFile>
{
    /// <summary>
    /// Create a new <see cref="OutputFilesDropDownView"/> instance.
    /// </summary>
    public OutputFilesDropDownView() 
        : base(
            file => file.Metadata.Name,
            file => file.Metadata.GetLongName(),
            false,
            (a, b) => a.Metadata.FileName == b.Metadata.FileName)
    {
    }

    /// <summary>
    /// Populate the view with a list of output files, grouping related items together.
    /// </summary>
    /// <param name="outputFiles">The output files to populate the view with.</param>
    public void Populate(IEnumerable<OutputFile> outputFiles)
    {
        PopulateGrouped(outputFiles);
    }
}
