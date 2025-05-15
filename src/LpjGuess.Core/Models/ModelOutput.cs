using LpjGuess.Core.Interfaces;

namespace LpjGuess.Core.Models;

/// <summary>
/// A model output file.
/// </summary>
public class ModelOutput : IDataSource
{
    /// <summary>
    /// Output file type.
    /// </summary>
    public string OutputFileType { get; set; }

    /// <summary>
    /// Name of the column used for the x-axis.
    /// </summary>
    public string XAxisColumn { get; set; }

    /// <summary>
    /// Name of the column used for the y-axis.
    /// </summary>
    public string YAxisColumn { get; set; }

    /// <summary>
    /// Create a new <see cref="ModelOutput"/> instance.
    /// </summary>
    public ModelOutput()
    {
        OutputFileType = string.Empty;
        XAxisColumn = string.Empty;
        YAxisColumn = string.Empty;
    }

    /// <summary>
    /// Create a new <see cref="ModelOutput"/> instance with specified parameters.
    /// </summary>
    /// <param name="outputFileType">Output file type.</param>
    /// <param name="xAxisColumn">Name of the column used for the x-axis.</param>
    /// <param name="yAxisColumn">Name of the column used for the y-axis.</param>
    public ModelOutput(
        string outputFileType,
        string xAxisColumn,
        string yAxisColumn
    )
    {
        OutputFileType = outputFileType;
        XAxisColumn = xAxisColumn;
        YAxisColumn = yAxisColumn;
    }
}
