using Gtk;
using GLib;

namespace LpjGuess.Frontend.Utility.Gtk;

/// <summary>
/// Commits an entry value on Enter or when keyboard focus leaves the entry.
/// </summary>
public sealed class EntryCommitter : IDisposable
{
    private readonly Entry entry;
    private readonly EventControllerFocus focus;
    private readonly Action<string> commit;
    private readonly Func<string, string?>? validate;
    private string committedValue;
    private string? pendingValue;
    private bool disposed;

    /// <summary>
    /// Create a commit controller for an entry.
    /// </summary>
    public EntryCommitter(
        Entry entry,
        Action<string> commit,
        Func<string, string?>? validate = null)
    {
        this.entry = entry;
        this.commit = commit;
        this.validate = validate;
        committedValue = entry.GetText();
        focus = EventControllerFocus.New();
        entry.AddController(focus);
        entry.OnActivate += OnActivate;
        focus.OnLeave += OnLeave;
    }

    /// <summary>
    /// Populate the entry without raising a commit.
    /// </summary>
    public void SetText(string value)
    {
        pendingValue = null;
        committedValue = value;
        entry.SetText(value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        entry.OnActivate -= OnActivate;
        focus.OnLeave -= OnLeave;
        entry.RemoveController(focus);
        focus.Dispose();
    }

    private void OnActivate(Entry sender, EventArgs args) => QueueCommit();

    private void OnLeave(EventControllerFocus sender, EventArgs args) => QueueCommit();

    private void QueueCommit()
    {
        if (disposed)
            return;

        string value = entry.GetText();
        if (value == committedValue || value == pendingValue)
            return;

        pendingValue = value;
        GLib.Functions.IdleAdd(0, () =>
        {
            CommitPending();
            return false;
        });
    }

    private void CommitPending()
    {
        string? value = pendingValue;
        pendingValue = null;
        if (disposed || value is null || value == committedValue)
            return;

        string? validationError = validate?.Invoke(value);
        if (validationError is not null)
        {
            entry.AddCssClass("error");
            entry.TooltipText = validationError;
            return;
        }

        // Mark the value committed before invoking the callback. The callback
        // is allowed to rebuild and dispose the entry's containing view.
        string previousValue = committedValue;
        committedValue = value;
        try
        {
            commit(value);
            if (!disposed)
            {
                entry.RemoveCssClass("error");
                entry.TooltipText = null;
            }
        }
        catch (Exception error)
        {
            committedValue = previousValue;
            if (!disposed)
            {
                entry.AddCssClass("error");
                entry.TooltipText = error.Message;
            }
        }
    }
}
