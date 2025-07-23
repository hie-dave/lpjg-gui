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
    /// Dictionary mapping experiment objects to their sidebar widgets.
    /// </summary>
    private readonly Dictionary<Experiment, Label> sidebarWidgets;

    /// <summary>
    /// Create a new <see cref="ExperimentsView"/> instance.
    /// </summary>
    public ExperimentsView() : base(RenderLabel)
    {
        AddText = "Add Experiment";
        sidebarWidgets = new Dictionary<Experiment, Label>();
    }

    /// <inheritdoc />
    public void Populate(IEnumerable<(Experiment, IExperimentView)> experiments)
    {
        Populate(experiments.Select(e => (e.Item1, e.Item2.GetWidget())));
    }

    /// <inheritdoc />
    public void Rename(Experiment experiment, string newName)
    {
        sidebarWidgets[experiment].SetText(newName);
    }

    /// <inheritdoc />
    protected override Widget CreateWidget(Experiment data)
    {
        Widget widget = base.CreateWidget(data);
        if (widget is Box box && box.GetFirstChild() is Label label)
            sidebarWidgets[data] = label;
        else
            Console.WriteLine($"Unable to initialise experiment view sidebar widget, likely due to changes in the base class");
        return widget;
    }

    /// <summary>
    /// Create a widget to be displayed in the sidebar for the given experiment.
    /// </summary>
    /// <param name="experiment">The experiment to render a widget for.</param>
    /// <returns>A widget to be displayed in the sidebar.</returns>
    private static Label RenderLabel(Experiment experiment)
    {
        return Label.New(experiment.Name);
    }
}
