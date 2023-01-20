using System.Reflection;

namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface for a presenter which interfaces with an <see cref="IPropertyView"/>.
/// </summary>
public interface IPropertyPresenter : IPresenter<IPropertyView>
{
	/// <summary>
	/// Check if this presenter can present the specified property.
	/// </summary>
	/// <param name="property">Property metadata.</param>
	public bool CanPresent(PropertyInfo property);
}
