using Gtk;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A dropdown widget which displays entities of the specified type {T} as
/// strings.
/// </summary>
public class StringDropDownView<T> : DropDownView<T, Label>
{
    /// <summary>
    /// A function which renders a value to a string which will be displayed in
    /// the dropdown.
    /// </summary>
    private readonly Func<T, string> renderer;

    /// <summary>
    /// Create a new <see cref="StringDropDownView{T}"/> instance.
    /// </summary>
    /// <param name="renderer">
    /// A function which renders a value to a string which will be displayed in
    /// the dropdown.
    /// </param>
    public StringDropDownView(Func<T, string> renderer) : base()
    {
        this.renderer = renderer;
    }

    /// <inheritdoc/>
    protected override void BindWidget(T item, Label widget)
    {
        widget.SetText(renderer(item));
    }

    /// <inheritdoc/>
    protected override Label CreateWidget()
    {
        return new Label() { Halign = Align.Start };
    }
}

/// <summary>
/// A dropdown widget which displays strings as-is.
/// </summary>
public class StringDropDownView : StringDropDownView<string>
{
    /// <summary>
    /// Create a new <see cref="StringDropDownView"/> instance.
    /// </summary>
    public StringDropDownView() : base(x => x)
    {
    }
}
