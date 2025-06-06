using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;
using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing.Style;
using Newtonsoft.Json;

namespace LpjGuess.Core.Models.Graphing.Series;

/// <summary>
/// A base class for all series types.
/// </summary>
public abstract class SeriesBase : ISeries
{
    /// <inheritdoc />
    public string Title { get; set; }

    /// <inheritdoc />
    public IStyleProvider<Colour> ColourProvider { get; set; }

    /// <inheritdoc />
    public IDataSource DataSource { get; set; }

    /// <inheritdoc />
    public AxisPosition XAxisPosition { get; set; }

    /// <inheritdoc />
    public AxisPosition YAxisPosition { get; set; }

    /// <summary>
    /// Create a new <see cref="SeriesBase"/> instance.
    /// </summary>
    /// <param name="title">The title of the series.</param>
    /// <param name="colourProvider">The colour provider for the series.</param>
    /// <param name="dataSource">The data source for the series.</param>
    /// <param name="xAxisPosition">The position of the X axis for the series.</param>
    /// <param name="yAxisPosition">The position of the Y axis for the series.</param>
    [JsonConstructor]
    public SeriesBase(
        string title,
        IStyleProvider<Colour> colourProvider,
        IDataSource dataSource,
        AxisPosition xAxisPosition,
        AxisPosition yAxisPosition)
    {
        Title = title;
        ColourProvider = colourProvider;
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
