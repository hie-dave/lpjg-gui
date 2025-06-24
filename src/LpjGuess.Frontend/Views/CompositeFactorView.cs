using Gtk;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view that allows the user to edit a CompositeFactor..
/// </summary>
public class CompositeFactorView : ViewBase<Box>, ICompositeFactorView
{
    /// <inheritdoc />
    public Event<IModelChange<CompositeFactor>> OnChanged { get; private init; }

    /// <inheritdoc />
    public Event OnAddFactor { get; private init; }

    /// <inheritdoc />
    public Event<string> OnRemoveFactor { get; private init; }

    /// <summary>
    /// Create a new <see cref="CompositeFactorView"/> instance.
    /// </summary>
    public CompositeFactorView() : base(new Box())
    {
        OnChanged = new Event<IModelChange<CompositeFactor>>();
        OnAddFactor = new Event();
        OnRemoveFactor = new Event<string>();
    }

    /// <inheritdoc />
    public void Populate(string name, IEnumerable<INamedView> factorViews)
    {
    }
}
