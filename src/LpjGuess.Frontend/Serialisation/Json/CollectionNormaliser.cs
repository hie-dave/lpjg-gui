using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;

namespace LpjGuess.Frontend.Serialisation.Json;

/// <summary>
/// This serialisation binder serialises all IEnumerable{T} as List{T}.
/// </summary>
public class CollectionNormaliser : ISerializationBinder
{
    /// <summary>
    /// The fallback serialisation binder.
    /// </summary>
	private readonly ISerializationBinder wrappedBinder;

    /// <summary>
    /// Create a new <see cref="CollectionNormaliser"/> instance.
    /// </summary>
    /// <param name="wrappedBinder">The fallback serialisation binder.</param>
	public CollectionNormaliser(ISerializationBinder wrappedBinder)
	{
		this.wrappedBinder = wrappedBinder;
	}

    /// <inheritdoc />
	public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
	{
	    // Serialization
		if (IsCollectionExpressionUnspeakableType(serializedType, out Type? elementType))
		{
			serializedType = typeof(List<>).MakeGenericType(elementType);
		}

		wrappedBinder.BindToName(serializedType, out assemblyName, out typeName);
	}

    /// <inheritdoc />
	public Type BindToType(string? assemblyName, string typeName)
	{
    	// Deserialization
		return wrappedBinder.BindToType(assemblyName, typeName);
	}

	private static bool IsCollectionExpressionUnspeakableType(
		Type type,
		[NotNullWhen(returnValue: true)]
		out Type? elementType
	)
	{
        elementType = null;

        if (!type.IsGenericType)
            return false;

        // Check if type is compiler-generated
        if (type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
            return false;

        // Check if implements IEnumerable<T>
        var ienumerableInterface = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (ienumerableInterface == null)
            return false;

        elementType = ienumerableInterface.GetGenericArguments()[0];
        return true;
	}
}
