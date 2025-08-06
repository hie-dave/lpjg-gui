using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Factorial;

namespace LpjGuess.Core.Models.Graphing.Style.Identifiers;

/// <summary>
/// A series identifier which identifies a series by its experiment (only
/// applicable for series generated from model outputs).
/// </summary>
public class ExperimentIdentifier : ISeriesIdentifier
{
    /// <inheritdoc />
    public SeriesIdentityBase Identify(SeriesContext context)
    {
        return new StringIdentity(context.ExperimentName);
    }

    /// <summary>
    /// Create a series identity for a given experiment.
    /// </summary>
    /// <param name="experiment">The experiment.</param>
    /// <returns>The series identity.</returns>
    public SeriesIdentityBase Identify(Experiment experiment)
    {
        return new StringIdentity(experiment.Name);
    }

    /// <inheritdoc />
    public StyleVariationStrategy GetStrategy() => StyleVariationStrategy.ByExperiment;
}
