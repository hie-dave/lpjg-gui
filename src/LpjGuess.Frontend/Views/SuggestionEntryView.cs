using Gtk;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Utility.Gtk;
using static GObject.Object;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A free-text entry with filtered suggestions displayed in a popover.
/// </summary>
public sealed class SuggestionEntryView : ViewBase<Box>
{
    private const int maxVisibleSuggestions = 8;

    private readonly Entry entry;
    private readonly EntryCommitter committer;
    private readonly Popover suggestionsPopover;
    private readonly ListBox suggestionsList;
    private readonly Label hint;
    private readonly Dictionary<ListBoxRow, string> rowValues;
    private IReadOnlyList<string> suggestions;
    private bool updating;

    /// <summary>
    /// Raised when text is committed by Enter, focus loss, or selecting a
    /// suggestion.
    /// </summary>
    public Event<string> OnCommitted { get; }

    /// <summary>
    /// The current entry text.
    /// </summary>
    public string Text => entry.GetText();

    /// <summary>
    /// Create a free-text suggestion entry.
    /// </summary>
    /// <param name="placeholder">Placeholder shown in the entry.</param>
    /// <param name="showHint">Whether to explain that suggestions are optional.</param>
    public SuggestionEntryView(string placeholder, bool showHint = false)
        : base(Box.New(Orientation.Vertical, 3))
    {
        suggestions = [];
        rowValues = [];
        OnCommitted = new Event<string>();

        entry = new Entry() { Hexpand = true, PlaceholderText = placeholder };
        committer = new EntryCommitter(entry, Commit);
        entry.OnNotify += OnEntryNotify;

        suggestionsList = new ListBox()
        {
            SelectionMode = SelectionMode.Single
        };
        suggestionsList.AddCssClass("navigation-sidebar");
        suggestionsList.OnRowActivated += OnSuggestionActivated;

        ScrolledWindow suggestionsScroll = new()
        {
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic,
            MinContentWidth = 320,
            MaxContentHeight = 260,
            PropagateNaturalHeight = true
        };
        suggestionsScroll.Child = suggestionsList;

        suggestionsPopover = Popover.New();
        // Autohide popovers are modal in GTK and perform an input grab when
        // shown. Completion is informational while the user types, so it must
        // remain non-modal and leave keyboard focus in the entry.
        suggestionsPopover.Autohide = false;
        suggestionsPopover.CanFocus = false;
        suggestionsPopover.FocusOnClick = false;
        suggestionsPopover.HasArrow = false;
        suggestionsPopover.SetChild(suggestionsScroll);
        suggestionsPopover.SetParent(entry);

        hint = new Label()
        {
            Halign = Align.Start,
            Wrap = true,
            Xalign = 0,
            Visible = showHint
        };
        hint.SetText("Type any name. Suggestions are derived from the selected instruction files.");
        hint.AddCssClass(StyleClasses.Subtitle);

        widget.Append(entry);
        widget.Append(hint);
    }

    /// <summary>
    /// Set the current text without raising a commit event.
    /// </summary>
    public void SetText(string value)
    {
        updating = true;
        try
        {
            committer.SetText(value);
            suggestionsPopover.Popdown();
        }
        finally
        {
            updating = false;
        }
    }

    /// <summary>
    /// Set optional completion suggestions. Arbitrary text remains valid.
    /// </summary>
    public void SetSuggestions(IEnumerable<string> values)
    {
        suggestions = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (entry.HasFocus)
            UpdateSuggestions();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        entry.OnNotify -= OnEntryNotify;
        suggestionsList.OnRowActivated -= OnSuggestionActivated;
        committer.Dispose();
        ClearSuggestionRows();
        suggestionsPopover.Unparent();
        suggestionsPopover.Dispose();
        OnCommitted.Dispose();
        base.Dispose();
    }

    private void OnEntryNotify(GObject.Object sender, NotifySignalArgs args)
    {
        if (!updating && args.Pspec.GetName() == "text")
            UpdateSuggestions();
    }

    private void UpdateSuggestions()
    {
        ClearSuggestionRows();

        string query = entry.GetText().Trim();
        if (query.Length == 0)
        {
            suggestionsPopover.Popdown();
            return;
        }

        List<string> matches = suggestions
            .Where(value => value.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(value => value.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Take(maxVisibleSuggestions)
            .ToList();

        if (matches.Count == 1 &&
            string.Equals(matches[0], query, StringComparison.OrdinalIgnoreCase))
        {
            suggestionsPopover.Popdown();
            return;
        }

        foreach (string match in matches)
        {
            Label label = new() { Halign = Align.Start, Xalign = 0 };
            label.SetText(match);
            ListBoxRow row = new() { Child = label };
            rowValues[row] = match;
            suggestionsList.Append(row);
        }

        // This method is reached from the entry's text notification for user
        // edits. Gtk.Entry may report HasFocus=false when focus is held by its
        // internal text widget, so using HasFocus here suppresses valid popup
        // requests on some GTK versions.
        if (matches.Count > 0)
            suggestionsPopover.Popup();
        else
            suggestionsPopover.Popdown();
    }

    private void ClearSuggestionRows()
    {
        suggestionsList.RemoveAll();
        foreach (ListBoxRow row in rowValues.Keys)
            row.Dispose();
        rowValues.Clear();
    }

    private void OnSuggestionActivated(
        ListBox sender,
        ListBox.RowActivatedSignalArgs args)
    {
        if (!rowValues.TryGetValue(args.Row, out string? value))
            return;

        SetText(value);
        OnCommitted.Invoke(value);
    }

    private void Commit(string value)
    {
        suggestionsPopover.Popdown();
        OnCommitted.Invoke(value);
    }
}
