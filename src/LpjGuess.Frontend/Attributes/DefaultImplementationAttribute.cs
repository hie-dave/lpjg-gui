namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// Attribute to mark a type as the default implementation of an interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DefaultImplementationAttribute : Attribute
{
}
