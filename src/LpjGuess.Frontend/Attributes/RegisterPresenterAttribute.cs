namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// Attribute to mark a type as a model presenter to be registered with the
/// dependency injection container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RegisterPresenterAttribute : Attribute
{
    /// <summary>
    /// The type against which this presenter is registered in the dependency
    /// injection container.
    /// </summary>
    public Type InterfaceType { get; private init; }

    /// <summary>
    /// Gets the type of the model that this presenter is for.
    /// </summary>
    public Type ModelType { get; private init; }

    /// <summary>
    /// Create a new <see cref="RegisterPresenterAttribute"/> instance.
    /// </summary>
    /// <param name="modelType">The type of the model that this presenter is for.</param>
    /// <param name="interfaceType">The type against which this presenter is registered in the dependency injection container.</param>
    public RegisterPresenterAttribute(Type modelType, Type interfaceType)
    {
        ModelType = modelType;
        InterfaceType = interfaceType;
    }
}
