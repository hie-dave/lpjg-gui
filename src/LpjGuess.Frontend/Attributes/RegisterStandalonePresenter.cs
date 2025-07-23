namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// Attribute to mark a type as a standalone presenter (which doesn't manage a
/// model) to be registered with the dependency injection container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RegisterStandalonePresenterAttribute : Attribute
{
    /// <summary>
    /// The type against which this presenter is registered in the dependency
    /// injection container.
    /// </summary>
    public Type InterfaceType { get; private init; }

    /// <summary>
    /// Create a new <see cref="RegisterStandalonePresenterAttribute"/> instance.
    /// </summary>
    /// <param name="interfaceType">The type against which this presenter is registered in the dependency injection container.</param>
    public RegisterStandalonePresenterAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }
}
