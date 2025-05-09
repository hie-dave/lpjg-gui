using System.Reflection;
using System.Runtime.Remoting;
using Gio;
using GObject;
using GObject.Internal;
using Gtk;
using Object = GObject.Object;
using ObjectHandle = GObject.Internal.ObjectHandle;

namespace LpjGuess.Frontend.Views.Helpers;

/// <summary>
/// A strongly-typed <see cref="ListModel"/> implementation.
/// </summary>
/// <typeparam name="T">The type of objects to be stored in the model.</typeparam>
public class GenericListModel<T> : Object, ListModel where T : Object
{
    /// <summary>
    /// Name of the signal emitted when the model is changed.
    /// </summary>
    private const string changedSignal = "items-changed";

    /// <summary>
    /// The list of items in the model.
    /// </summary>
    private readonly List<T> items;

    /// <summary>
    /// Create a new <see cref="GenericListModel{T}"/> instance.
    /// </summary>
    public GenericListModel() : base(ObjectHandle.For<GenericListModel<T>>(Array.Empty<ConstructArgument>()))
    {
        items = new List<T>();
    }

    /// <summary>
    /// Get the object at the specified position.
    /// </summary>
    /// <param name="position">The position of the object to retrieve.</param>
    /// <returns>The object at the specified position.</returns>
    public nint GetItem(uint position)
    {
        return items[(int)position].Handle.DangerousGetHandle();
    }

    /// <summary>
    /// Get the type of objects in the model.
    /// </summary>
    public GObject.Type GetItemType()
    {
        MethodInfo? method = typeof(T).GetMethod(nameof(GetGType));
        if (method == null)
            return GetGType();
        return (GObject.Type)method.Invoke(null, [])!;
    }

    /// <summary>
    /// Get the number of items in the model.
    /// </summary>
    public uint GetNItems() => (uint)items.Count;

    /// <summary>
    /// Get the object at the specified position.
    /// </summary>
    /// <param name="position">The position of the object to retrieve.</param>
    /// <returns>The object at the specified position.</returns>
    public GObject.Object? GetObject(uint position)
    {
        if (position > int.MaxValue || position >= items.Count)
            // This will be called by native code - it's tempting to throw here,
            // but that could end badly.
            return null;
        return items[(int)position];
    }

    /// <summary>
    /// Populate the model with data.
    /// </summary>
    /// <param name="values">The values to populate the model with.</param>
    public void Populate(IEnumerable<T> values)
    {
        // Remove any existing data.
        int nremoved = items.Count;
        items.Clear();

        // Add the new data.
        items.AddRange(values);

        // Signal to any listeners that the model has changed.
        ItemsChanged(0, (uint)nremoved, (uint)items.Count);
    }

    /// <summary>
    /// Emits the ItemsChanged signal to notify listeners that the model has
    /// changed.
    /// </summary>
    /// <param name="position">The position at which items were changed.</param>
    /// <param name="removed">The number of items that were removed.</param>
    /// <param name="added">The number of items that were added.</param>
    public void ItemsChanged(uint position, uint removed, uint added)
    {
        // Emit the ItemsChanged signal to notify listeners that the model has changed.
        // This should be called after modifying the items collection.
        // GObject.Signal.EmitSignal(this, changedSignal, position, removed, added);
        
    }
}
