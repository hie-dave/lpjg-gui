using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays a list of experiments.
/// </summary>
public class ExperimentsView : DynamicStackSidebar<Experiment>, IExperimentsView
{
    /// <summary>
    /// Create a new <see cref="ExperimentsView"/> instance.
    /// </summary>
    public ExperimentsView() : base(RenderLabel)
    {
        AddText = "Add Experiment";
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<(Experiment, IExperimentView)> experiments)
    {
        Populate(experiments.Select(e => (e.Item1, e.Item2.GetWidget())));
    }

    /// <summary>
    /// Create a widget to be displayed in the sidebar for the given experiment.
    /// </summary>
    /// <param name="experiment">The experiment to render a widget for.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    private static Widget RenderLabel(Experiment experiment)
    {
        return Label.New(experiment.Name);
    }
}
