using LpjGuess.Core.Interfaces.Graphing.Style;
using LpjGuess.Core.Models.Graphing;
using LpjGuess.Core.Models.Graphing.Style;
using LpjGuess.Frontend.Attributes;
using LpjGuess.Frontend.Commands;
using LpjGuess.Frontend.Delegates;
using LpjGuess.Frontend.Interfaces.Commands;
using LpjGuess.Frontend.Interfaces.Presenters;
using LpjGuess.Frontend.Interfaces.Views;

namespace LpjGuess.Frontend.Presenters;

/// <summary>
/// A presenter which controls a filter view to allow the user to view and
/// customize a single filter.
/// </summary>
[RegisterPresenter(typeof(DataFilter), typeof(IFilterPresenter))]
public class FilterPresenter : PresenterBase<IFilterView, DataFilter>, IFilterPresenter
{
    /// <summary>
    /// The set of valid series identities for the strategy being used, which
    /// the user may filter out.
    /// </summary>
    private List<SeriesIdentityBase> identities;

    /// <summary>
    /// Event raised when the filter is changed by the user.
    /// </summary>
    public Event<ICommand> OnFilterChanged { get; private init; }

    /// <summary>
    /// Create a new <see cref="FilterPresenter"/> instance.
    /// </summary>
    /// <param name="view">The view to present to.</param>
    /// <param name="filter">The filter being edited.</param>
    /// <param name="registry">The command registry.</param>
    public FilterPresenter(
        IFilterView view,
        DataFilter filter,
        ICommandRegistry registry)
        : base(view, filter, registry)
    {
        OnFilterChanged = new Event<ICommand>();
        identities = [];
        view.OnFilterChanged.ConnectTo(OnSelectionChanged);
    }

    /// <summary>
    /// Populate the view with the given series identities.
    /// </summary>
    /// <param name="identities">The series identities to populate the view with.</param>
    public void Populate(IEnumerable<SeriesIdentityBase> identities)
    {
        this.identities = identities.ToList();
        PopulateView();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        OnFilterChanged.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Populate the view with the given series identities.
    /// </summary>
    private void PopulateView()
    {
        // An identity is enabled if the filtered values does not contain it.
        view.Populate(model.Strategy, identities.Select(i => (i.ToString(), !model.FilteredValues.Contains(i))));
    }

    /// <summary>
    /// Called when the filter is changed by the user.
    /// </summary>
    /// <param name="identities">The identities of enabled series.</param>
    private void OnSelectionChanged(IEnumerable<string> identities)
    {
        IEnumerable<SeriesIdentityBase> enabled = identities.Select(GetIdentity);
        IEnumerable<SeriesIdentityBase> disabled = this.identities
            .Where(i => !enabled.Contains(i));
        PropertyChangeCommand<DataFilter, List<SeriesIdentityBase>> command = new(
            model,
            model.FilteredValues,
            disabled.ToList(),
            (filter, value) => filter.FilteredValues = value);
        OnFilterChanged.Invoke(command);
    }

    /// <summary>
    /// Get the series identity with the given name.
    /// </summary>
    /// <param name="name">The name of the series identity.</param>
    /// <returns>The series identity.</returns>
    /// <exception cref="ArgumentException">Thrown if the series identity is not found.</exception>
    private SeriesIdentityBase GetIdentity(string name)
    {
        SeriesIdentityBase? identity = identities.FirstOrDefault(i => i.ToString() == name);
        if (identity == null)
            throw new ArgumentException($"Identity {name} not found");
        return identity;
    }
}
