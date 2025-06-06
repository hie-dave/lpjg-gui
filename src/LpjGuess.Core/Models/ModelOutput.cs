using Dave.Benchmarks.Core.Models;
using Dave.Benchmarks.Core.Models.Entities;
using Dave.Benchmarks.Core.Models.Importer;
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
    /// The columns used for the y-axis. This is stored as a list internally to
    /// allow for easier serialisation.
    /// </summary>
    [NonSerialized]
    private List<string> columns = new();

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
    public IEnumerable<string> YAxisColumns
    {
        get => columns;
        set => columns = value.ToList();
    }

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
        YAxisColumns = [];
        InstructionFiles = new List<string>();
    }

    /// <summary>
    /// Create a new <see cref="ModelOutput"/> instance with specified parameters.
    /// </summary>
    /// <param name="outputFileType">Output file type.</param>
    /// <param name="xAxisColumn">Name of the column used for the x-axis.</param>
    /// <param name="yAxisColumns">Names of the columns used for the y-axis.</param>
    /// <param name="instructionFiles">Instruction files for which data should be displayed.</param>
    public ModelOutput(
        string outputFileType,
        string xAxisColumn,
        IEnumerable<string> yAxisColumns,
        IEnumerable<string> instructionFiles
    )
    {
        OutputFileType = outputFileType;
        XAxisColumn = xAxisColumn;
        YAxisColumns = yAxisColumns;
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
        // FIXME: This is a hack.
        return AxisType.Linear;
    }

    /// <inheritdoc />
    public string GetXAxisTitle()
    {
        return XAxisColumn;
    }

    /// <inheritdoc />
    public string GetYAxisTitle()
    {
        OutputFileMetadata metadata = OutputFileDefinitions.GetMetadata(OutputFileType);
        IEnumerable<Unit> units = YAxisColumns.Select(metadata.Layers.GetUnits);
        int nunits = units.Select(u => u.Name).Distinct().Count();
        if (nunits > 1)
            return metadata.Name;
        else if (nunits == 1)
            // Simplify if all selected layers have the same units.
            return $"{metadata.Name} ({units.First().Name})";
        else
            return $"{metadata.Name} (various units)";
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

    /// <inheritdoc />
    public int GetNumSeries()
    {
        return InstructionFiles.Count * YAxisColumns.Count();
    }

    /// <summary>
    /// Get the allowed style variation strategies for the given aggregation level.
    /// </summary>
    /// <param name="level">The aggregation level.</param>
    /// <returns>The allowed style variation strategies.</returns>
    private IEnumerable<StyleVariationStrategy> GetAllowedStrategies(AggregationLevel level)
    {
        List<StyleVariationStrategy> strategies = [
            StyleVariationStrategy.BySeries
        ];

        if (level >= AggregationLevel.Gridcell)
            strategies.Add(StyleVariationStrategy.ByGridcell);

        if (level >= AggregationLevel.Stand)
            strategies.Add(StyleVariationStrategy.ByStand);

        if (level >= AggregationLevel.Patch)
            strategies.Add(StyleVariationStrategy.ByPatch);

        if (level >= AggregationLevel.Individual)
        {
            strategies.Add(StyleVariationStrategy.ByIndividual);
            strategies.Add(StyleVariationStrategy.ByPft);
        }

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
