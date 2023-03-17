using Adw;

namespace LpjGuess.Frontend.Interfaces.Views;

/// <summary>
/// An interface to a view encapsulating a preferences group.
/// </summary>
public interface IGroupView : IView
{
	/// <summary>
	/// Get the preferences group encapsulated by this view.
	/// </summary>
	PreferencesGroup GetGroup();
}
