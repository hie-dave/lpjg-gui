using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Runner.Models;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays an experiment.
/// </summary>
public class ExperimentView : ViewBase<Box>, IExperimentView
{
    /// <inheritdoc />
    public Event<IModelChange<Experiment>> OnChanged { get; }

    /// <summary>
    /// Create a new <see cref="ExperimentView"/> instance.
    /// </summary>
    public ExperimentView() : base(new Box())
    {
        OnChanged = new Event<IModelChange<Experiment>>();
        widget.SetOrientation(Orientation.Vertical);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        OnChanged.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public void Populate(
        string name,
        string description,
        string runner,
        IEnumerable<string> instructionFiles,
        IEnumerable<string> pfts,
        IEnumerable<ParameterGroup> factorials)
    {
        // TBI
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        // TBI
    }
}
