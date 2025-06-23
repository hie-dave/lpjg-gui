using System.Diagnostics.CodeAnalysis;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// A delegate for methods which follow the TryParse pattern.
    /// </summary>
    public delegate bool TryParseFunc<in TSource, TResult>(TSource source, out TResult result);

	/// <summary>
	/// Attempt to convert all items in a sequence. If any conversion fails,
	/// null is returned.
	/// </summary>
	/// <param name="source">The sequence to be converted.</param>
	/// <param name="parser">The conversion function.</param>
    /// <param name="result">The converted sequence, or null if any of the
    /// conversions fail.</param>
	/// <typeparam name="TSource">The type of the items in the source
	/// sequence.</typeparam>
	/// <typeparam name="TResult">The target type of the conversion.</typeparam>
	/// <returns>
	/// A sequence of converted items, or null if any of the conversions fail.
	/// </returns>
	public static bool TrySelect<TSource, TResult>(
		this IEnumerable<TSource> source,
		TryParseFunc<TSource, TResult> parser,
        [NotNullWhen(true)] out IEnumerable<TResult>? result)
	{
		List<TResult> list = [];
		foreach (var item in source)
		{
			if (!parser(item, out TResult parsedItem))
			{
                result = null;
				return false;
			}
			list.Add(parsedItem);
		}
        result = list;
		return true;
	}

    /// <summary>
    /// Returns the cartesian product of the specified sequences.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> AllCombinations<T>(this IEnumerable<IEnumerable<T>> items)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        return items.Aggregate(
            emptyProduct,
            (product, collection) =>
            from accumulated in product
            from item in collection
            select accumulated.Append(item));
    }
}
