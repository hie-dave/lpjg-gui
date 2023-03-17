using System.Collections;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for enumerable types.
/// </summary>
public static class EnumerableExtensions
{
	/// <summary>
	/// Append an element to the collection without modifying the original
	/// collection.
	/// </summary>
	/// <param name="enumerable">A collection.</param>
	/// <param name="appendee">The item to be appended to the collection.</param>
	public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T appendee)
	{
		foreach (T element in enumerable)
			yield return element;
		yield return appendee;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="appendee"></param>
	/// <returns></returns>
	public static IEnumerable Append(this IEnumerable collection, object appendee)
	{
		foreach (var element in collection)
			yield return element;
		yield return appendee;
	}

	/// <summary>
	/// Remove an item from the collection, if it exists in the collection.
	/// </summary>
	/// <param name="collection">A collection.</param>
	/// <param name="removee">The item to be removed from the collection.</param>
	public static IEnumerable Remove(this IEnumerable collection, object removee)
	{
		foreach (object element in collection)
		{
			if (element == removee)
				continue;
			yield return element;
		}
	}
}
