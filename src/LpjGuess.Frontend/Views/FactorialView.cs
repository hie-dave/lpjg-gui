using Gtk;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Edits the strategy and dimensions used to generate experiment simulations.
/// </summary>
public class FactorialView : ViewBase<Box>, IFactorialView
{
    private const int spacing = 8;

    private readonly CheckButton combineAllButton;
    private readonly CheckButton oneAtATimeButton;
    private readonly Label variationCount;
    private readonly ListBoxRevealerView factorsContainer;
    private readonly Dictionary<IView, IValueGeneratorView> factorViews;
    private bool updating;

    /// <inheritdoc />
    public Event<IModelChange<FactorialGenerator>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddFactor => factorsContainer.OnAdd;

    /// <inheritdoc />
    public Event<IView> OnRemoveFactor => factorsContainer.OnRemove;

    /// <summary>
    /// Create a factorial design view.
    /// </summary>
    public FactorialView(ILoggerFactory loggerFactory) : base(Box.New(Orientation.Vertical, spacing))
    {
        OnChanged = new Event<IModelChange<FactorialGenerator>>();

        combineAllButton = CheckButton.New();
        combineAllButton.SetLabel("Combine every value");
        oneAtATimeButton = CheckButton.New();
        oneAtATimeButton.SetLabel("Vary one parameter at a time");
        oneAtATimeButton.SetGroup(combineAllButton);

        factorsContainer = new ListBoxRevealerView(
            loggerFactory.CreateLogger<ListBoxNavigatorView>());
        factorsContainer.AddText = "Add parameter variation or scenario set";
        factorViews = [];

        variationCount = new Label() { Halign = Align.Start, Xalign = 0 };
        variationCount.AddCssClass(StyleClasses.Subtitle);

        widget.Append(CreateStrategySection());
        widget.Append(variationCount);
        widget.Append(factorsContainer.GetWidget());

        combineAllButton.OnToggled += OnStrategyChanged;
        oneAtATimeButton.OnToggled += OnStrategyChanged;
    }

    /// <inheritdoc />
    public void Populate(bool fullFactorial, IEnumerable<IValueGeneratorView> factorViews)
    {
        List<IValueGeneratorView> views = factorViews.ToList();
        updating = true;
        try
        {
            combineAllButton.Active = fullFactorial;
            oneAtATimeButton.Active = !fullFactorial;
        }
        finally
        {
            updating = false;
        }

        this.factorViews.Clear();
        foreach (IValueGeneratorView view in views)
            this.factorViews[view.View] = view;
        UpdateSummary();
        factorsContainer.Populate(views);
    }

    /// <inheritdoc />
    public void RenameFactor(IView view, string name)
        => factorsContainer.Rename(view, name);

    /// <inheritdoc />
    public void UpdateFactor(IValueGeneratorView factorView)
    {
        factorViews[factorView.View] = factorView;
        factorsContainer.Update(factorView);
        UpdateSummary();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        combineAllButton.OnToggled -= OnStrategyChanged;
        oneAtATimeButton.OnToggled -= OnStrategyChanged;
        factorViews.Clear();
        OnChanged.Dispose();
        base.Dispose();
    }

    private Widget CreateStrategySection()
    {
        Label heading = Label.New("How should variation rows be combined?");
        heading.Halign = Align.Start;
        heading.AddCssClass(StyleClasses.Heading);

        Label combineHelp = CreateHelp(
            "Creates every combination across rows. For 3 × 4 × 2 levels, this creates 24 simulations.");
        Label oneHelp = CreateHelp(
            "Creates one simulation for each configured level, leaving other variation rows unchanged.");

        Box combine = Box.New(Orientation.Vertical, 2);
        combine.Append(combineAllButton);
        combine.Append(combineHelp);

        Box oneAtATime = Box.New(Orientation.Vertical, 2);
        oneAtATime.Append(oneAtATimeButton);
        oneAtATime.Append(oneHelp);

        Box section = Box.New(Orientation.Vertical, 6);
        section.Append(heading);
        section.Append(combine);
        section.Append(oneAtATime);
        return section;
    }

    private static Label CreateHelp(string text)
    {
        Label label = new() { Halign = Align.Start, Wrap = true, Xalign = 0 };
        label.SetText(text);
        label.MarginStart = 24;
        label.AddCssClass(StyleClasses.Subtitle);
        return label;
    }

    private void UpdateSummary()
    {
        int levels = factorViews.Values.Sum(view => Math.Max(0, view.LevelCount));
        variationCount.SetText(
            $"{factorViews.Count:N0} variation rows · {levels:N0} configured levels");
    }

    private void OnStrategyChanged(CheckButton sender, EventArgs args)
    {
        if (updating || !sender.Active)
            return;

        try
        {
            bool fullFactorial = ReferenceEquals(sender, combineAllButton);
            OnChanged.Invoke(new ModelChangeEventArgs<FactorialGenerator, bool>(
                generator => generator.FullFactorial,
                (generator, value) => generator.FullFactorial = value,
                fullFactorial));
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
