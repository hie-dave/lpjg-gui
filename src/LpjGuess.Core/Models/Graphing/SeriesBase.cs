using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Interfaces.Graphing;

namespace LpjGuess.Core.Models.Graphing;

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
}
