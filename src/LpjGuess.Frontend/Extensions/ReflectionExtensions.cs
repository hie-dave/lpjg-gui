using System.Reflection;

namespace LpjGuess.Frontend.Extensions;

/// <summary>
/// Extension methods for reflection types.
/// </summary>
public static class ReflectionExtensions
{
	/// <summary>
	/// Find all implementations of an interface.
	/// </summary>
	/// <param name="interfaceType">An interface.</param>
	public static IEnumerable<Type> FindImplementations(this Type interfaceType)
	{
		if (!interfaceType.IsInterface)
			throw new InvalidOperationException($"{interfaceType.Name} is not an interface");

		Assembly assembly = Assembly.GetExecutingAssembly();
		foreach (Type type in assembly.GetTypes())
			if (interfaceType.IsAssignableFrom(type))
				yield return type;
	}

	/// <summary>
	/// Get a generic method on the given type or throw if not found.
	/// </summary>
	/// <param name="type">The type</param>
	/// <param name="methodName">Method name.</param>
	/// <param name="typeArguments">Generic type parameters.</param>
	public static MethodInfo GetGenericMethod(this Type type, string methodName, params Type[] typeArguments)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.FlattenHierarchy;

		string fullMethodName = $"{type.Name}.{methodName}<{string.Join(", ", typeArguments.Select(t => t.Name))}>";
		try
		{
			MethodInfo? method = type.GetMethod(methodName, flags);
			if (method == null)
				throw new InvalidOperationException("Method does not exist");
			method = method.MakeGenericMethod(typeArguments);
			return method;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to find method {fullMethodName}", error);
		}
	}

	/// <summary>
	/// Create an instance of the specified type using the default constructor.
	/// If no default constructor exists, an exception will be thrown.
	/// </summary>
	/// <param name="type">The type to be constructed.</param>
	public static object CreateInstance(this Type type)
	{
		try
		{
			object? result = Activator.CreateInstance(type);
			if (result == null)
				throw new InvalidOperationException($"{type.Name} contains no default constructor");
			return result;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to create instance of {type.Name}", error);
		}
	}

	/// <summary>
	/// Invoke the specified method using the given arguments, and return the
	/// result. Throw if the method returns null.
	/// </summary>
	/// <param name="method">The method to be invoked.</param>
	/// <param name="instance">The object instance on which the method is to be called. Set to null if method is static.</param>
	/// <param name="arguments">Arguments to be passed into the method.</param>
	public static object InvokeNonNullable(this MethodInfo method, object? instance, params object[] arguments)
	{
		try
		{
			object? result = method.Invoke(instance, arguments);
			if (result == null)
				throw new Exception($"Method {method.Name} returned null");
			return result;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to invoke method {method.Name}", error);
		}
	}
}
