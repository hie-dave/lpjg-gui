using LpjGuess.Frontend.Interfaces.Commands;

namespace LpjGuess.Frontend.Commands;

/// <summary>
/// A command to add an element to a collection.
/// </summary>
/// <typeparam name="TElement">The type of the element to add.</typeparam>
public class AddElementCommand<TElement> : ICommand
{
    /// <summary>
    /// The collection to add the element to.
    /// </summary>
    private readonly ICollection<TElement> collection;

    /// <summary>
    /// The element to add to the collection.
    /// </summary>
    private readonly TElement element;

    /// <summary>
    /// Create a new <see cref="AddElementCommand{TElement}"/> instance.
    /// </summary>
    /// <param name="collection">The collection to add the element to.</param>
    /// <param name="element">The element to add to the collection.</param>
    public AddElementCommand(ICollection<TElement> collection, TElement element)
    {
        this.collection = collection;
        this.element = element;
    }

    /// <inheritdoc />
    public void Execute()
    {
        collection.Add(element);
    }

    /// <inheritdoc />
    public string GetDescription()
    {
        return $"Add element {element} to collection {collection}";
    }

    /// <inheritdoc />
    public void Undo()
    {
        collection.Remove(element);
    }
}
