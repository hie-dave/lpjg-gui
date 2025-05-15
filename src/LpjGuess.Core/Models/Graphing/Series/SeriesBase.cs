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
    }

    /// <inheritdoc />
    public IEnumerable<AxisRequirements> GetAxisRequirements()
    {
        return
        [
            new AxisRequirements(DataSource.GetXAxisType(), XAxisPosition, DataSource.GetXAxisTitle()),
            new AxisRequirements(DataSource.GetYAxisType(), YAxisPosition, Title)
        ];
    }
}
