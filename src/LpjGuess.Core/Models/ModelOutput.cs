using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Services;
using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;

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

    /// <inheritdoc />
    public string GetName()
    {
        try
        {
            return OutputFileDefinitions.GetMetadata(OutputFileType).Name;
        }
        catch (Exception)
        {
            // Fallback to output file type for unknown files types.
            return OutputFileType;
        }
    }

    /// <inheritdoc />
    public IEnumerable<StyleVariationStrategy> GetAllowedStyleVariationStrategies()
    {
        OutputFileMetadata meta = OutputFileDefinitions.GetMetadata(OutputFileType);
        return GetAllowedStrategies(meta.Level);
    }

    /// <summary>
    /// Get the allowed style variation strategies for the given aggregation level.
    /// </summary>
    /// <param name="level">The aggregation level.</param>
    /// <returns>The allowed style variation strategies.</returns>
    private IEnumerable<StyleVariationStrategy> GetAllowedStrategies(AggregationLevel level)
    {
        List<StyleVariationStrategy> strategies = new();

        if (level >= AggregationLevel.Gridcell)
            strategies.Add(StyleVariationStrategy.ByGridcell);

        if (level >= AggregationLevel.Stand)
            strategies.Add(StyleVariationStrategy.ByStand);

        if (level >= AggregationLevel.Patch)
            strategies.Add(StyleVariationStrategy.ByPatch);

        if (level >= AggregationLevel.Individual)
            strategies.Add(StyleVariationStrategy.ByIndividual);

        return strategies;
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
