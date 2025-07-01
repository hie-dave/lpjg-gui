namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// Attribute to mark a generic type for registration with the dependency
/// injection container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RegisterGenericTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the types that should be used as type parameters when registering this generic type.
    /// </summary>
    public Type[] SupportedTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterGenericTypeAttribute"/> class.
    /// </summary>
    /// <param name="supportedTypes">The types that should be used as type parameters when registering this generic type.</param>
    public RegisterGenericTypeAttribute(params Type[] supportedTypes)
    {
        SupportedTypes = supportedTypes;
    }
}
