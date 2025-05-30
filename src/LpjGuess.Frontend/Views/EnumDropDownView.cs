using LpjGuess.Core.Extensions;

namespace LpjGuess.Frontend.Views;

/// <summary>
/// A dropdown view for an enum type.
/// </summary>
public class EnumDropDownView<T> : StringDropDownView<T> where T : struct, Enum
{
    /// <summary>
    /// Create a new <see cref="EnumDropDownView{T}"/> instance.
    /// </summary>
    public EnumDropDownView() : base(SanitiseName)
    {
        // Enum.GetName() can return null in two scenarios:
        // 1. The value passed in doesn't correspond to any defined enum value.
        // 2. It's a flags enum, and we're passing in a combination of flags.
        // Since we're doing neither of these, it's safe to use the ! operator.
        IEnumerable<T> values = Enum.GetValues(typeof(T)).Cast<T>();
        Populate(values);
    }

    /// <summary>
    /// Sanitises an enum name for display.
    /// </summary>
    /// <param name="name">The enum name to sanitise.</param>
    /// <returns>The sanitised enum name.</returns>
    private static string SanitiseName(T name)
    {
        return Enum.GetName(name)!.PascalToHumanCase();
    }
}
