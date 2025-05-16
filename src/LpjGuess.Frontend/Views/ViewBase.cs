using Gtk;
using LpjGuess.Frontend.Interfaces;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// Base class for views.
/// </summary>
public abstract class ViewBase<T> : IView where T : Widget
{
    /// <summary>
    /// The widget which is returned by <see cref="GetWidget()"/>.
    /// </summary>
    protected T widget;

    /// <summary>
    /// Create a new <see cref="ViewBase{T}"/> instance.
    /// </summary>
    /// <param name="widget">The widget to be returned by <see cref="GetWidget()"/>.</param>
    protected ViewBase(T widget)
    {
        this.widget = widget;
        this.widget.Name = SanitiseName(GetType().Name);
    }

    /// <inheritdoc/>
    public Widget GetWidget() => widget;

    /// <summary>
    /// Dispose of native resources.
    /// </summary>
    public virtual void Dispose()
    {
        widget.Dispose();
    }

    private static string SanitiseName(string name)
    {
        return name.ToString()
                   .Replace(".", string.Empty)
                   .Replace("+", string.Empty)
                   .Replace("`", string.Empty)
                   .Replace("[", "_")
                   .Replace("]", string.Empty)
                   .Replace(" ", string.Empty)
                   .Replace(",", "_");
    }
}
