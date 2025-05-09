using GObject.Internal;

namespace LpjGuess.Frontend.Views.Helpers;

/// <summary>
/// A GObject wrapper for a generic type.
/// </summary>
public class GenericGObject<T> : GObject.Object where T : class
{
    /// <summary>
    /// The wrappee.
    /// </summary>
    public T Instance { get; private init; }

    /// <summary>
    /// Create a new <see cref="GenericGObject{T}"/> instance.
    /// </summary>
    public GenericGObject(T instance) : base(ObjectHandle.For<GenericGObject<T>>([]))
    {
        Instance = instance;
    }
}
