using Gtk;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Frontend.Classes;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Displays experiment setup, parameter variations, and a bounded run preview.
/// </summary>
public class ExperimentView : ViewBase<Notebook>, IExperimentView
{
    private const int spacing = 8;

    private readonly Entry nameEntry;
    private readonly Entry descriptionEntry;
    private readonly EntryCommitter nameCommitter;
    private readonly EntryCommitter descriptionCommitter;
    private readonly StringDropDownView runnerDropDown;
    private readonly InstructionFileSelectionView insFileView;
    private readonly FlowBoxSelectionView pftView;
    private readonly CheckButton allPftsCheck;
    private readonly Box factorialContainer;
    private readonly Label designStatus;
    private readonly Label simulationCount;
    private readonly Label modelRunCount;
    private readonly Label previewNote;
    private readonly Label validationOutcome;
    private readonly Box issuesContainer;
    private readonly CustomColumnView<SimulationPreviewRow> simulationView;

    private bool updatingPfts;

    /// <inheritdoc />
    public Event<IModelChange<Experiment>> OnChanged { get; }

    /// <summary>
    /// Create a new experiment view.
    /// </summary>
    public ExperimentView() : base(new Notebook())
    {
        OnChanged = new Event<IModelChange<Experiment>>();

        nameEntry = new Entry() { Hexpand = true };
        descriptionEntry = new Entry() { Hexpand = true };
        nameCommitter = new EntryCommitter(nameEntry, OnNameChanged);
        descriptionCommitter = new EntryCommitter(descriptionEntry, OnDescriptionChanged);
        runnerDropDown = new StringDropDownView();
        insFileView = new InstructionFileSelectionView();
        pftView = new FlowBoxSelectionView();
        allPftsCheck = CheckButton.NewWithLabel("Keep PFT enablement from the base instruction files");
        factorialContainer = Box.New(Orientation.Vertical, spacing);

        designStatus = CreateSummaryValue();
        simulationCount = CreateSummaryValue();
        modelRunCount = CreateSummaryValue();
        previewNote = new Label() { Halign = Align.Start, Wrap = true };
        validationOutcome = new Label()
        {
            Halign = Align.Start,
            Wrap = true,
            Xalign = 0
        };
        validationOutcome.AddCssClass(StyleClasses.Heading);
        issuesContainer = Box.New(Orientation.Vertical, 8);

        simulationView = new CustomColumnView<SimulationPreviewRow>();
        ConfigureSimulationColumns([]);

        runnerDropDown.Populate(Configuration.Instance.Runners.Select(r => r.Name));

        widget.AppendPage(CreateSetupPage(), Label.New("Setup"));
        widget.AppendPage(CreateVariationsPage(), Label.New("Parameter variations"));
        widget.AppendPage(CreatePreviewPage(), Label.New("Preview"));

        ConnectEvents();
    }

    /// <inheritdoc />
    public void Populate(
        string name,
        string description,
        string runner,
        IEnumerable<(string, bool)> instructionFiles,
        bool inheritPfts,
        IEnumerable<(string Name, bool EnabledByDefault, bool Selected)> pfts)
    {
        nameCommitter.SetText(name);
        descriptionCommitter.SetText(description);
        runnerDropDown.Select(runner);
        UpdateInstructionFiles(instructionFiles);
        UpdatePfts(inheritPfts, pfts);
    }

    /// <inheritdoc />
    public void PopulatePreview(
        ExperimentDesignAnalysis analysis,
        IEnumerable<SimulationDescription> simulations,
        bool truncated)
    {
        designStatus.SetText(analysis.IsValid ? "Ready" : "Needs attention");
        simulationCount.SetText(FormatCount(analysis.SimulationCount, analysis.CountOverflowed));
        modelRunCount.SetText(FormatCount(analysis.ModelRunCount, analysis.CountOverflowed));

        ClearChildren(issuesContainer);
        validationOutcome.RemoveCssClass("error");
        validationOutcome.RemoveCssClass("warning");
        validationOutcome.RemoveCssClass("success");

        int errors = analysis.Issues.Count(issue =>
            issue.Severity == ExperimentDesignIssueSeverity.Error);
        int warnings = analysis.Issues.Count(issue =>
            issue.Severity == ExperimentDesignIssueSeverity.Warning);
        if (errors > 0)
        {
            validationOutcome.SetText(
                $"{errors:N0} validation error{(errors == 1 ? string.Empty : "s")} must be resolved");
            validationOutcome.AddCssClass("error");
        }
        else if (warnings > 0)
        {
            validationOutcome.SetText(
                $"{warnings:N0} validation warning{(warnings == 1 ? string.Empty : "s")} found");
            validationOutcome.AddCssClass("warning");
        }
        else
        {
            validationOutcome.SetText("No validation issues found");
            validationOutcome.AddCssClass("success");
        }

        List<ExperimentDesignIssue> sortedIssues = analysis.Issues
            .OrderBy(issue => issue.Severity switch
            {
                ExperimentDesignIssueSeverity.Error => 0,
                ExperimentDesignIssueSeverity.Warning => 1,
                _ => 2
            })
            .ToList();
        for (int issueIndex = 0; issueIndex < sortedIssues.Count; issueIndex++)
        {
            ExperimentDesignIssue issue = sortedIssues[issueIndex];
            string prefix = issue.Severity switch
            {
                ExperimentDesignIssueSeverity.Error => "Error",
                ExperimentDesignIssueSeverity.Warning => "Warning",
                _ => "Information"
            };
            Label severity = new Label()
            {
                Halign = Align.Start,
                Xalign = 0
            };
            severity.SetText(prefix);
            severity.AddCssClass(StyleClasses.Heading);
            if (issue.Severity == ExperimentDesignIssueSeverity.Error)
                severity.AddCssClass("error");
            else if (issue.Severity == ExperimentDesignIssueSeverity.Warning)
                severity.AddCssClass("warning");

            Label message = new Label()
            {
                Halign = Align.Start,
                Wrap = true,
                Xalign = 0
            };
            message.SetText(issue.Message);

            Box issueRow = Box.New(Orientation.Vertical, 2);
            issueRow.MarginTop = issueRow.MarginBottom = 4;
            issueRow.Append(severity);
            issueRow.Append(message);
            issuesContainer.Append(issueRow);

            if (issueIndex < sortedIssues.Count - 1)
                issuesContainer.Append(Separator.New(Orientation.Horizontal));
        }

        List<SimulationDescription> simulationList = simulations.ToList();
        List<string> targets = simulationList
            .SelectMany(simulation => simulation.Changes)
            .Select(change => change.DisplayTarget)
            .Distinct()
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
        ConfigureSimulationColumns(targets);

        List<SimulationPreviewRow> rows = [];
        foreach (SimulationDescription simulation in simulationList)
        {
            string changes = simulation.Changes.Count == 0
                ? "No parameter changes"
                : string.Join(
                    "\n",
                    simulation.Changes.Select(change =>
                        $"{change.DisplayTarget} = {change.Value}"));
            Dictionary<string, string> values = simulation.Changes
                .GroupBy(change => change.DisplayTarget)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last().Value);
            rows.Add(new SimulationPreviewRow(simulation.Name, changes, values));
        }
        simulationView.Populate(rows);

        previewNote.SetText(truncated
            ? "Showing the first 100 simulations. Counts above include the full design."
            : rows.Count == 0
                ? "No simulations are generated by the current design."
                : "Preview includes all generated simulations.");
    }

    /// <inheritdoc />
    public void UpdateInstructionFiles(IEnumerable<(string, bool)> instructionFiles)
    {
        List<(string, bool)> files = instructionFiles.ToList();
        insFileView.Populate(
            files.Select(file => file.Item1),
            files.Where(file => file.Item2).Select(file => file.Item1));
    }

    /// <inheritdoc />
    public void SetFactorialView(IFactorialView factorialView)
    {
        ClearChildren(factorialContainer, dispose: false);
        factorialContainer.Append(factorialView.GetWidget());
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        DisconnectEvents();
        nameCommitter.Dispose();
        descriptionCommitter.Dispose();
        OnChanged.Dispose();
        base.Dispose();
    }

    private Widget CreateSetupPage()
    {
        Box content = Box.New(Orientation.Vertical, spacing);
        content.MarginTop = content.MarginBottom = 12;
        content.MarginStart = content.MarginEnd = 12;

        Grid details = new Grid() { RowSpacing = spacing, ColumnSpacing = spacing };
        int row = 0;
        AddControl(details, ref row, "Name", nameEntry);
        AddControl(details, ref row, "Description", descriptionEntry);
        AddControl(details, ref row, "Runner", runnerDropDown.GetWidget());

        content.Append(CreateSection(
            "Experiment details",
            "Name the experiment and select the execution environment.",
            details));
        content.Append(CreateSection(
            "Base instruction files",
            "Each generated simulation is applied to every selected file.",
            insFileView));

        Box pfts = Box.New(Orientation.Vertical, spacing);
        pfts.Append(allPftsCheck);
        pfts.Append(pftView.GetWidget());
        content.Append(CreateSection(
            "Plant functional types",
            "By default, generated files preserve each base file's existing PFT enablement. "
            + "Clear the checkbox to apply one explicit PFT selection to every generated file.",
            pfts));

        return WrapInScroll(content);
    }

    private Widget CreateVariationsPage()
    {
        Box content = Box.New(Orientation.Vertical, spacing);
        content.MarginTop = content.MarginBottom = 12;
        content.MarginStart = content.MarginEnd = 12;
        content.Append(CreateSection(
            "Simulation design",
            "Add parameter variations or multi-parameter scenario sets. Select a row to edit it.",
            factorialContainer));
        return WrapInScroll(content);
    }

    private Widget CreatePreviewPage()
    {
        Box content = Box.New(Orientation.Vertical, spacing);
        content.MarginTop = content.MarginBottom = 12;
        content.MarginStart = content.MarginEnd = 12;

        Grid summary = new Grid() { RowSpacing = spacing, ColumnSpacing = 18 };
        int row = 0;
        AddControl(summary, ref row, "Status", designStatus);
        AddControl(summary, ref row, "Simulations", simulationCount);
        AddControl(summary, ref row, "Total model runs", modelRunCount);

        content.Append(CreateSection(
            "Design summary",
            "Counts update as the simulation design or selected instruction files change.",
            summary));
        content.Append(CreateSection(
            "Validation",
            "Validation updates automatically when the experiment configuration changes.",
            CreateValidationPanel()));

        Box preview = Box.New(Orientation.Vertical, spacing);
        preview.Append(previewNote);
        ScrolledWindow tableScroll = new ScrolledWindow()
        {
            HscrollbarPolicy = PolicyType.Automatic,
            VscrollbarPolicy = PolicyType.Automatic,
            MinContentHeight = 320,
            Vexpand = true
        };
        tableScroll.Child = simulationView;
        preview.Append(tableScroll);
        content.Append(CreateSection(
            "Generated simulations",
            "Each row is one simulation. Parameter targets are fully qualified and grouped within that simulation.",
            preview));

        ScrolledWindow page = WrapInScroll(content);
        page.Vexpand = true;
        return page;
    }

    private static Frame CreateSection(string title, string description, Widget child)
    {
        Label heading = Label.New(title);
        heading.Halign = Align.Start;
        heading.AddCssClass(StyleClasses.Heading);

        Label help = new Label() { Halign = Align.Start, Wrap = true, Xalign = 0 };
        help.SetText(description);
        help.AddCssClass(StyleClasses.Subtitle);

        Box box = Box.New(Orientation.Vertical, 6);
        box.MarginTop = box.MarginBottom = 8;
        box.MarginStart = box.MarginEnd = 8;
        box.Append(heading);
        box.Append(help);
        box.Append(child);

        Frame frame = new Frame();
        frame.SetChild(box);
        return frame;
    }

    private static ScrolledWindow WrapInScroll(Widget child)
    {
        return new ScrolledWindow()
        {
            Child = child,
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic
        };
    }

    private static void AddControl(Grid grid, ref int row, string title, Widget control)
    {
        Label label = Label.New($"{title}:");
        label.Halign = Align.Start;
        label.Valign = Align.Center;
        grid.Attach(label, 0, row, 1, 1);
        grid.Attach(control, 1, row, 1, 1);
        row++;
    }

    private static Label CreateSummaryValue()
        => new() { Halign = Align.Start, Xalign = 0 };

    private Widget CreateValidationPanel()
    {
        Label heading = Label.New("Current configuration");
        heading.Halign = Align.Start;
        heading.AddCssClass(StyleClasses.Subtitle);

        Box panel = Box.New(Orientation.Vertical, 8);
        panel.MarginTop = panel.MarginBottom = 10;
        panel.MarginStart = panel.MarginEnd = 10;
        panel.Append(heading);
        panel.Append(validationOutcome);
        panel.Append(Separator.New(Orientation.Horizontal));
        panel.Append(issuesContainer);

        Frame frame = new Frame();
        frame.AddCssClass("card");
        frame.SetChild(panel);
        return frame;
    }

    private static Label CreateTableLabel()
        => new() { Halign = Align.Start, Xalign = 0, Selectable = false };

    private static Label CreateChangesTableLabel()
        => new() { Halign = Align.Start, Xalign = 0, Wrap = true, Selectable = false };

    private void ConfigureSimulationColumns(IReadOnlyList<string> targets)
    {
        simulationView.Clear();
        simulationView.AddColumn(
            "Simulation",
            CreateTableLabel,
            (row, label) => label.SetText(row.Simulation));

        int limit = Math.Max(0, Configuration.Instance.SimulationPreviewParameterColumnLimit);
        if (targets.Count > 0 && targets.Count <= limit)
        {
            foreach (string target in targets)
            {
                simulationView.AddColumn(
                    target,
                    CreateTableLabel,
                    (row, label) => label.SetText(row.GetValue(target)));
            }
        }
        else
        {
            simulationView.AddColumn(
                "Parameter changes",
                CreateChangesTableLabel,
                (row, label) => label.SetText(row.Changes));
        }
    }

    private static string FormatCount(long count, bool overflowed)
        => overflowed && count == long.MaxValue ? "More than 9.2 quintillion" : count.ToString("N0");

    private static void ClearChildren(Box box, bool dispose = true)
    {
        Widget? child;
        while ((child = box.GetFirstChild()) != null)
        {
            box.Remove(child);
            if (dispose)
                child.Dispose();
        }
    }

    private void UpdatePfts(
        bool inheritPfts,
        IEnumerable<(string Name, bool EnabledByDefault, bool Selected)> pfts)
    {
        List<(string Name, bool EnabledByDefault, bool Selected)> options = pfts.ToList();

        updatingPfts = true;
        try
        {
            pftView.Populate(options.Select(option => option.Name));
            pftView.Select(options
                .Where(option => inheritPfts ? option.EnabledByDefault : option.Selected)
                .Select(option => option.Name));

            allPftsCheck.Active = inheritPfts;
            pftView.GetWidget().Sensitive = !inheritPfts;
        }
        finally
        {
            updatingPfts = false;
        }
    }

    private void ConnectEvents()
    {
        runnerDropDown.OnSelectionChanged.ConnectTo(OnRunnerChanged);
        insFileView.OnSelectionChanged.ConnectTo(OnInsFilesChanged);
        allPftsCheck.OnToggled += OnAllPftsToggled;
        pftView.OnSelectionChanged.ConnectTo(OnPftsChanged);
    }

    private void DisconnectEvents()
    {
        runnerDropDown.OnSelectionChanged.DisconnectFrom(OnRunnerChanged);
        insFileView.OnSelectionChanged.DisconnectFrom(OnInsFilesChanged);
        allPftsCheck.OnToggled -= OnAllPftsToggled;
        pftView.OnSelectionChanged.DisconnectFrom(OnPftsChanged);
    }

    private void OnInsFilesChanged(IEnumerable<string> enabledFiles)
    {
        HashSet<string> enabled = enabledFiles.ToHashSet();
        List<string> disabled = insFileView.GetSelection()
            .Where(file => !enabled.Contains(file.Item1))
            .Select(file => file.Item1)
            .ToList();

        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, IEnumerable<string>>(
            experiment => experiment.DisabledInsFiles,
            (experiment, files) => experiment.DisabledInsFiles = files.ToList(),
            disabled));
    }

    private void OnRunnerChanged(string runnerName)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
            experiment => experiment.Runner,
            (experiment, value) => experiment.Runner = value,
            runnerName));
    }

    private void OnAllPftsToggled(CheckButton sender, EventArgs args)
    {
        if (updatingPfts)
            return;

        pftView.GetWidget().Sensitive = !sender.Active;
        IEnumerable<string> selected = sender.Active
            ? []
            : pftView.GetSelectedItems();
        RaisePftsChanged(selected);
    }

    private void OnPftsChanged(IEnumerable<string> selected)
    {
        if (!updatingPfts && !allPftsCheck.Active)
            RaisePftsChanged(selected);
    }

    private void RaisePftsChanged(IEnumerable<string> selected)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, IEnumerable<string>>(
            experiment => experiment.Pfts,
            (experiment, values) => experiment.Pfts = values.ToList(),
            selected.ToList()));
    }

    private void OnDescriptionChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
            experiment => experiment.Description,
            (experiment, value) => experiment.Description = value,
            value));
    }

    private void OnNameChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<Experiment, string>(
            experiment => experiment.Name,
            (experiment, value) => experiment.Name = value,
            value));
    }
}
