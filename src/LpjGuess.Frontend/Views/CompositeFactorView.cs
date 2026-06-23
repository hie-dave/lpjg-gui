using Gtk;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;
using LpjGuess.Frontend.Utility.Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view that allows the user to edit a CompositeFactor..
/// </summary>
public class CompositeFactorView : ViewBase<Box>, ICompositeFactorView
{
    private class RowWrapper : ViewBase<ListBoxRow>
    {
        private readonly Button deleteButton;
        private readonly Box container;

        public IView Child { get; private init; }

        public Event OnRemoveFactor { get; private init; }

        public RowWrapper(IView child) : base(new ListBoxRow())
        {
            OnRemoveFactor = new Event();
            Child = child;

            deleteButton = Button.NewFromIconName(Icons.Delete);
            deleteButton.AddCssClass(StyleClasses.DestructiveAction);
            deleteButton.OnClicked += OnDeleteFactor;
            deleteButton.Valign = Align.Center;
            deleteButton.Halign = Align.End;

            container = Box.New(Orientation.Horizontal, 6);
            container.Append(Child.GetWidget());
            container.Append(deleteButton);

            widget.Child = container;
        }

        public override void Dispose()
        {
            deleteButton.OnClicked -= OnDeleteFactor;
            OnRemoveFactor.Dispose();
            // We don't own the main widget so we can't dispose of it here.
            container.Remove(Child.GetWidget());
            base.Dispose();
        }

        private void OnDeleteFactor(Button sender, EventArgs args)
        {
            try
            {
                OnRemoveFactor.Invoke();
            }
            catch (Exception error)
            {
                MainView.Instance.ReportError(error);
            }
        }
    }

    /// <summary>
    /// The spacing between widgets in the view.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The entry for the scenario name.
    /// </summary>
    private readonly Entry nameEntry;
    private readonly EntryCommitter nameCommitter;

    /// <summary>
    /// The list box containing the factor views.
    /// </summary>
    private readonly ListBox listBox;

    /// <summary>
    /// The rows in the listbox.
    /// </summary>
    private readonly List<RowWrapper> rows;

    /// <summary>
    /// The button that allows the user to add factors.
    /// </summary>
    private readonly Button addButton;

    /// <inheritdoc />
    public Event<IModelChange<CompositeFactor>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddFactor { get; private init; }

    /// <inheritdoc />
    public Event<IView> OnRemoveFactor { get; private init; }

    /// <summary>
    /// Create a new <see cref="CompositeFactorView"/> instance.
    /// </summary>
    public CompositeFactorView() : base(new Box())
    {
        rows = new List<RowWrapper>();

        OnChanged = new Event<IModelChange<CompositeFactor>>();
        OnAddFactor = new Event();
        OnRemoveFactor = new Event<IView>();

        nameEntry = new Entry() { Hexpand = true };
        nameEntry.PlaceholderText = "e.g. Baseline, Moderate warming";
        nameCommitter = new EntryCommitter(nameEntry, OnNameChanged);

        listBox = new ListBox();
        listBox.SelectionMode = SelectionMode.None;

        addButton = Button.NewWithLabel("Add parameter change");
        addButton.AddCssClass(StyleClasses.SuggestedAction);
        addButton.OnClicked += OnAddFactorClicked;

        widget.SetOrientation(Orientation.Vertical);
        widget.Spacing = spacing;
        Grid details = new Grid() { RowSpacing = spacing, ColumnSpacing = spacing };
        Label nameLabel = Label.New("Scenario name:");
        nameLabel.Halign = Align.Start;
        details.Attach(nameLabel, 0, 0, 1, 1);
        details.Attach(nameEntry, 1, 0, 1, 1);
        widget.Append(details);
        widget.Append(listBox);
        widget.Append(addButton);
    }

    /// <inheritdoc />
    public void Populate(string name, IEnumerable<INamedView> factorViews)
    {
        nameCommitter.SetText(name);

        // Remove existing rows from the listbox and dispose of them.
        listBox.RemoveAll();
        rows.ForEach(r => r.Dispose());
        rows.Clear();

        foreach (INamedView view in factorViews)
        {
            RowWrapper row = new RowWrapper(view.View);
            row.OnRemoveFactor.ConnectTo(() => OnRemoveFactor.Invoke(view.View));
            rows.Add(row);
            listBox.Append(row.GetWidget());
        }
    }

    /// <inheritdoc />
    public void Rename(string name)
    {
        nameCommitter.SetText(name);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        addButton.OnClicked -= OnAddFactorClicked;
        nameCommitter.Dispose();
        rows.ForEach(row => row.Dispose());
        rows.Clear();
        OnChanged.Dispose();
        OnAddFactor.Dispose();
        OnRemoveFactor.Dispose();
        base.Dispose();
    }

    private void OnNameChanged(string value)
    {
        OnChanged.Invoke(new LpjGuess.Frontend.Events.ModelChangeEventArgs<CompositeFactor, string>(
            scenario => scenario.Name,
            (scenario, name) => scenario.Name = name,
            value));
    }

    /// <summary>
    /// Called when the "add factor" button is clicked by the user.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Event arguments.</param>
    private void OnAddFactorClicked(Button sender, EventArgs args)
    {
        try
        {
            OnAddFactor.Invoke();
        }
        catch (Exception error)
        {
            MainView.Instance.ReportError(error);
        }
    }
}
