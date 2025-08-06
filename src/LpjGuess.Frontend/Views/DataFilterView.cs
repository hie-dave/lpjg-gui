using Gtk;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Events;
using LpjGuess.Frontend.Extensions;
using LpjGuess.Frontend.Interfaces.Events;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A view for editing a single graph filter.
/// </summary>
public class DataFilterView : ViewBase<Box>, IFilterView
{
    /// <summary>
    /// Horizontal spacing between widgets in the main box.
    /// </summary>
    private const int spacing = 6;

    /// <summary>
    /// The view for selecting values to be filtered out.
    /// </summary>
    private readonly ISelectionView filtersView;

    /// <inheritdoc/>
    public StyleVariationStrategy Strategy { get; private set; }

    /// <inheritdoc/>
    public Event<IEnumerable<string>> OnFilterChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="DataFilterView"/> instance.
    /// </summary>
    public DataFilterView() : base(Box.New(Orientation.Horizontal, spacing))
    {
        OnFilterChanged = new Event<IEnumerable<string>>();

        filtersView = new FlowBoxSelectionView();
        filtersView.OnSelectionChanged.ConnectTo(OnFilterChanged);

        widget.Append(filtersView.GetWidget());
    }

    /// <inheritdoc/>
    public void Populate(StyleVariationStrategy strategy, IEnumerable<(string identity, bool enabled)> identities)
    {
        Strategy = strategy;
        filtersView.Populate(identities.Select(i => i.identity));
        filtersView.Select(identities.Where(i => i.enabled).Select(i => i.identity));
    }
}
