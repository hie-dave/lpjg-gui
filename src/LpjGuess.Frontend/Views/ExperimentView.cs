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
    /// <summary>
    /// The view which displays the factorial data.
    /// </summary>
    public IFactorialView FactorialView { get; }

    /// <inheritdoc />
    public Event<IModelChange<Experiment>> OnChanged { get; }

    /// <summary>
    /// Create a new <see cref="ExperimentView"/> instance.
    /// </summary>
    public ExperimentView() : base(new Box())
    {
        FactorialView = new FactorialView();
        OnChanged = new Event<IModelChange<Experiment>>();
        widget.SetOrientation(Orientation.Vertical);

        // Pack child widgets.
        widget.Append(FactorialView.GetWidget());
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
        IEnumerable<string> pfts)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<string> instructionFiles)
    {
        throw new NotImplementedException();
    }
}
