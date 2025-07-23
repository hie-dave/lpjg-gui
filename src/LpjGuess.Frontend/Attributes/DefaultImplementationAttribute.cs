namespace LpjGuess.Frontend.Attributes;

/// <summary>
/// Attribute to mark a type as the default implementation of an interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultImplementationAttribute : Attribute
{
}
