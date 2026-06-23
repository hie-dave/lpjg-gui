using Gtk;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Edits a set of named multi-parameter scenario levels.
/// </summary>
public class SimpleFactorGeneratorView : ViewBase<Box>, ISimpleFactorGeneratorView
{
    private sealed class ScenarioRow : ViewBase<ListBoxRow>
    {
        private readonly Label title;
        private readonly Button removeButton;
        private readonly Box content;

        public IView ScenarioView { get; }
        public Event OnRemove { get; }

        public ScenarioRow(INamedView view) : base(new ListBoxRow())
        {
            ScenarioView = view.View;
            OnRemove = new Event();

            title = Label.New(view.Name);
            title.Halign = Align.Start;
            title.Hexpand = true;
            title.AddCssClass(StyleClasses.Heading);

            removeButton = Button.NewFromIconName(Icons.Delete);
            removeButton.AddCssClass(StyleClasses.DestructiveAction);
            removeButton.OnClicked += OnRemoveClicked;

            Box header = Box.New(Orientation.Horizontal, 6);
            header.Append(title);
            header.Append(removeButton);

            content = Box.New(Orientation.Vertical, 6);
            content.MarginTop = content.MarginBottom = 8;
            content.MarginStart = content.MarginEnd = 8;
            content.Append(header);
            content.Append(view.View.GetWidget());
            widget.Child = content;
        }

        public void Rename(string name) => title.SetText(name);

        public override void Dispose()
        {
            removeButton.OnClicked -= OnRemoveClicked;
            content.Remove(ScenarioView.GetWidget());
            OnRemove.Dispose();
            base.Dispose();
        }

        private void OnRemoveClicked(Button sender, EventArgs args)
            => OnRemove.Invoke();
    }

    private readonly Entry nameEntry;
    private readonly EntryCommitter nameCommitter;
    private readonly ListBox scenarios;
    private readonly Button addButton;
    private readonly List<ScenarioRow> rows;

    /// <inheritdoc />
    public Event<IModelChange<SimpleFactorGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddLevel { get; private init; }

    /// <inheritdoc />
    public Event<IView> OnRemoveLevel { get; private init; }

    /// <summary>
    /// Create a scenario-set editor.
    /// </summary>
    public SimpleFactorGeneratorView(ILoggerFactory factory)
        : base(Box.New(Orientation.Vertical, 8))
    {
        _ = factory;
        OnChanged = new Event<IModelChange<SimpleFactorGenerator>>();
        OnAddLevel = new Event();
        OnRemoveLevel = new Event<IView>();
        rows = [];

        nameEntry = new Entry() { Hexpand = true };
        nameCommitter = new EntryCommitter(nameEntry, OnNameChanged);
        scenarios = new ListBox() { SelectionMode = SelectionMode.None };
        addButton = Button.NewWithLabel("Add scenario");
        addButton.AddCssClass(StyleClasses.SuggestedAction);

        Grid details = new Grid() { RowSpacing = 6, ColumnSpacing = 6 };
        Label nameLabel = Label.New("Scenario set name:");
        nameLabel.Halign = Align.Start;
        details.Attach(nameLabel, 0, 0, 1, 1);
        details.Attach(nameEntry, 1, 0, 1, 1);

        Label help = new()
        {
            Halign = Align.Start,
            Wrap = true,
            Xalign = 0
        };
        help.SetText("Each scenario is one simulation level and may change several parameters together.");
        help.AddCssClass(StyleClasses.Subtitle);

        widget.Append(details);
        widget.Append(help);
        widget.Append(scenarios);
        widget.Append(addButton);

        addButton.OnClicked += OnAddClicked;
    }

    /// <inheritdoc />
    public void Populate(string name, IEnumerable<INamedView> factorLevelViews)
    {
        nameCommitter.SetText(name);
        scenarios.RemoveAll();
        rows.ForEach(row => row.Dispose());
        rows.Clear();

        foreach (INamedView view in factorLevelViews)
        {
            ScenarioRow row = new(view);
            row.OnRemove.ConnectTo(() => OnRemoveLevel.Invoke(view.View));
            rows.Add(row);
            scenarios.Append(row.GetWidget());
        }
    }

    /// <inheritdoc />
    public void Rename(IView view, string name)
    {
        ScenarioRow? row = rows.FirstOrDefault(row => ReferenceEquals(row.ScenarioView, view));
        row?.Rename(name);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        nameCommitter.Dispose();
        addButton.OnClicked -= OnAddClicked;
        rows.ForEach(row => row.Dispose());
        rows.Clear();
        OnChanged.Dispose();
        OnAddLevel.Dispose();
        OnRemoveLevel.Dispose();
        base.Dispose();
    }

    private void OnAddClicked(Button sender, EventArgs args)
        => OnAddLevel.Invoke();

    private void OnNameChanged(string value)
    {
        OnChanged.Invoke(new ModelChangeEventArgs<SimpleFactorGenerator, string>(
            factor => factor.Name,
            (factor, value) => factor.Name = value,
            value));
    }
}
