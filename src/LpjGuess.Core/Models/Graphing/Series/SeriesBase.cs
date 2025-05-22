using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;

namespace LpjGuess.Core.Models.Graphing.Series;

/// <summary>
/// A base class for all series types.
/// </summary>
public abstract class SeriesBase : ISeries
{
    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public string Colour { get; set; }

    /// <inheritdoc />
    public IDataSource DataSource { get; set; }

    /// <inheritdoc />
    public AxisPosition XAxisPosition { get; set; }

    /// <inheritdoc />
    public AxisPosition YAxisPosition { get; set; }

    /// <summary>
    /// Default constructor provided for serialisation purposes only. Don't use
    /// this.
    /// </summary>
    /// <remarks>
    /// TODO: refactor serialization and series init so this is not needed.
    /// </remarks>
    public SeriesBase()
    {
        Title = string.Empty;
        Colour = string.Empty;
        DataSource = null!;
        XAxisPosition = AxisPosition.Bottom;
        YAxisPosition = AxisPosition.Left;
    }

    /// <summary>
    /// Create a new <see cref="SeriesBase"/> instance.
    /// </summary>
    /// <param name="title">The title of the series.</param>
    /// <param name="colour">The colour of the series.</param>
    /// <param name="dataSource">The data source for the series.</param>
    /// <param name="xAxisPosition">The position of the X axis for the series.</param>
    /// <param name="yAxisPosition">The position of the Y axis for the series.</param>
    public SeriesBase(
        string title,
        string colour,
        IDataSource dataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition)
    {
        Title = title;
        Colour = colour;
        DataSource = dataSource;
        XAxisPosition = xAxisPosition;
        YAxisPosition = yAxisPosition;
    }

    /// <inheritdoc />
    public IEnumerable<AxisRequirements> GetAxisRequirements()
    {
        return
        [
            new AxisRequirements(DataSource.GetXAxisType(), XAxisPosition, DataSource.GetXAxisTitle()),
            new AxisRequirements(DataSource.GetYAxisType(), YAxisPosition, DataSource.GetYAxisTitle())
        ];
    }
}
