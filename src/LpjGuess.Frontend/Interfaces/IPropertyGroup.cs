namespace LpjGuess.Frontend.Interfaces;

/// <summary>
/// An interface to a group of properties.
/// </summary>
public interface IPropertyGroup
{
	/// <summary>
	/// Name of the property group.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Properties in this group.
	/// </summary>
	public IEnumerable<IPropertyPresenter> Presenters { get; }
}
