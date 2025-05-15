using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models.Graphing;

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
    /// Instruction files for which data should be displayed.
    /// </summary>
    public List<string> InstructionFiles { get; set; }

    /// <summary>
    /// Create a new <see cref="ModelOutput"/> instance.
    /// </summary>
    public ModelOutput()
    {
        OutputFileType = string.Empty;
        XAxisColumn = string.Empty;
        YAxisColumn = string.Empty;
        InstructionFiles = new List<string>();
    }

    /// <summary>
    /// Create a new <see cref="ModelOutput"/> instance with specified parameters.
    /// </summary>
    /// <param name="outputFileType">Output file type.</param>
    /// <param name="xAxisColumn">Name of the column used for the x-axis.</param>
    /// <param name="yAxisColumn">Name of the column used for the y-axis.</param>
    /// <param name="instructionFiles">Instruction files for which data should be displayed.</param>
    public ModelOutput(
        string outputFileType,
        string xAxisColumn,
        string yAxisColumn,
        IEnumerable<string> instructionFiles
    )
    {
        OutputFileType = outputFileType;
        XAxisColumn = xAxisColumn;
        YAxisColumn = yAxisColumn;
        InstructionFiles = instructionFiles.ToList();
    }

    /// <inheritdoc />
    public AxisType GetXAxisType()
    {
        return GetAxisType(XAxisColumn);
    }

    /// <inheritdoc />
    public AxisType GetYAxisType()
    {
        return GetAxisType(YAxisColumn);
    }

    /// <inheritdoc />
    public string GetXAxisTitle()
    {
        return XAxisColumn;
    }

    /// <inheritdoc />
    public string GetYAxisTitle()
    {
        return YAxisColumn;
    }

    /// <summary>
    /// Get the type of the given axis column.
    /// </summary>
    /// <param name="column">The column to get the type for.</param>
    /// <returns>The type of the column.</returns>
    private static AxisType GetAxisType(string column)
    {
        return column == "Date" ? AxisType.DateTime : AxisType.Linear;
    }
}
