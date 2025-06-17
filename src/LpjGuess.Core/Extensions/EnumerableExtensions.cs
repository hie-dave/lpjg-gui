namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
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
