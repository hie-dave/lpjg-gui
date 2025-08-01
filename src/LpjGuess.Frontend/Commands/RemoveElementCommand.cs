using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command to remove an element from a collection.
/// </summary>
/// <typeparam name="TElement">The type of the element to remove.</typeparam>
public class RemoveElementCommand<TElement> : ICommand
{
    /// <summary>
    /// The collection to remove the element from.
    /// </summary>
    private readonly IList<TElement> collection;

    /// <summary>
    /// The element to remove from the collection.
    /// </summary>
    private readonly TElement element;

    /// <summary>
    /// The index of the element in the collection.
    /// </summary>
    private readonly int index;

    /// <summary>
    /// Create a new <see cref="RemoveElementCommand{TElement}"/> instance.
    /// </summary>
    /// <param name="collection">The collection to remove the element from.</param>
    /// <param name="element">The element to remove from the collection.</param>
    public RemoveElementCommand(IList<TElement> collection, TElement element)
    {
        this.collection = collection;
        this.element = element;
        index = collection.IndexOf(element);
        if (index == -1)
            throw new ArgumentException($"Element {element} not found in collection {collection}");
    }

    /// <inheritdoc />
    public void Execute()
    {
        collection.Remove(element);
    }

    /// <inheritdoc />
    public string GetDescription()
    {
        return $"Remove element {element} from collection {collection}";
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (collection.Count >= index)
            collection.Insert(index, element);
        else
            // TODO: write a warning message? This ought not happen.
            collection.Add(element);
    }
}
