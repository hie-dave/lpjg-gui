using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Runner.Models;
using OxyPlot;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view which displays an experiment.
/// </summary>
public class ExperimentView : ViewBase<Box>, IExperimentView
{
    /// <summary>
    /// The spacing between widgets.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The grid containing the controls used to customise the experiment.
    /// </summary>
    private readonly Grid grid;

    /// <summary>
    /// The entry used to edit the experiment name.
    /// </summary>
    private readonly Entry nameEntry;

    /// <summary>
    /// The entry used to edit the experiment description.
    /// </summary>
    private readonly Entry descriptionEntry;

    /// <summary>
    /// The dropdown used to select the experiment runner.
    /// </summary>
    private readonly StringDropDownView runnerDropDown;

    /// <summary>
    /// A child widget which allows the user to select which instruction files
    /// are used in the experiment.
    /// </summary>
    private readonly InstructionFileSelectionView insFileView;

    /// <summary>
    /// Number of rows currently in the grid.
    /// </summary>
    private int nrow;

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
        nrow = 0;
        FactorialView = new FactorialView();
        OnChanged = new Event<IModelChange<Experiment>>();
        widget.SetOrientation(Orientation.Vertical);

        // Configure container.
        widget.Spacing = spacing;

        // Initialise and configure child widgets.
        grid = new Grid();
        grid.RowSpacing = spacing;
        grid.ColumnSpacing = spacing;

        nameEntry = new Entry() { Hexpand = true };
        descriptionEntry = new Entry() { Hexpand = true };
        runnerDropDown = new StringDropDownView();

        insFileView = new InstructionFileSelectionView();
        insFileView.OnSelectionChanged.ConnectTo(OnInsFilesChanged);

        // FIXME: this won't be updated when a new runner is created. Need to
        // think about how to propagate these sorts of changes through the GUI
        // (this applies to other areas too such as instruction files).
        runnerDropDown.Populate(Configuration.Instance.Runners.Select(r => r.Name));
        runnerDropDown.OnSelectionChanged.ConnectTo(OnRunnerChanged);

        AddControl("Name", nameEntry);
        AddControl("Description", descriptionEntry);
        AddControl("Runner", runnerDropDown.GetWidget());

        // Pack child widgets into the container.
        widget.Append(grid);

        // Label insFilesLabel = Label.New("Instruction Files");
        // insFilesLabel.Halign = Align.Start;
        // widget.Append(insFilesLabel);
        // widget.Append(insFileView);
        AddControl("Instruction Files", insFileView);

        // Pack child widgets.
        widget.Append(FactorialView.GetWidget());

        // Connect event handlers.
        ConnectEvents();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        OnChanged.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    public void Populate(
        string name,
        string description,
        string runner,
        IEnumerable<(string, bool)> instructionFiles,
        IEnumerable<string> pfts)
    {
        nameEntry.SetText(name);
        descriptionEntry.SetText(description);

        // If this runner doesn't exist, it won't be selected. This also won't
        // throw - nothing will happen.
        runnerDropDown.Select(runner);

        UpdateInstructionFiles(instructionFiles);
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<(string, bool)> instructionFiles)
    {
        insFileView.Populate(
            instructionFiles.Select(i => i.Item1),
            instructionFiles.Where(i => i.Item2).Select(i => i.Item1));
    }

    /// <summary>
    /// Add a control to the next row in the grid.
    /// </summary>
    /// <param name="title">The title of the control.</param>
    /// <param name="widget">The widget to add.</param>
    private void AddControl(string title, Widget widget)
    {
        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        grid.Attach(label, 0, nrow, 1, 1);
        grid.Attach(widget, 1, nrow, 1, 1);
        nrow++;
    }

    private void ConnectEvents()
    {
        nameEntry.OnActivate += OnNameChanged;
        descriptionEntry.OnActivate += OnDescriptionChanged;
    }

    private void DisconnectEvents()
    {
        nameEntry.OnActivate -= OnNameChanged;
        descriptionEntry.OnActivate -= OnDescriptionChanged;
    }

    /// <summary>
    /// Called when the instruction files are changed by the user.
    /// </summary>
    /// <param name="insFiles">The instruction files.</param>
    private void OnInsFilesChanged(IEnumerable<string> insFiles)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, IEnumerable<string>>(
            e => e.InstructionFiles,
            (e, i) => e.InstructionFiles = i.ToList(),
            insFiles));
    }

    /// <summary>
    /// Called when the runner is changed by the user.
    /// </summary>
    /// <param name="runnerName">The name of the runner.</param>
    private void OnRunnerChanged(string runnerName)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
            e => e.Runner,
            (e, r) => e.Runner = r,
            runnerName));
    }

    /// <summary>
    /// Called when the description is changed by the user.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnDescriptionChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
                e => e.Description,
                (e, d) => e.Description = d,
                sender.GetText()));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }

    /// <summary>
    /// Called when the name is changed by the user.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event data.</param>
    private void OnNameChanged(Entry sender, EventArgs args)
    {
        try
        {
            OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
                e => e.Name,
                (e, n) => e.Name = n,
                sender.GetText()));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
