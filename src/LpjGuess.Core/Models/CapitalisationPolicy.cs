namespace LpjGuess.Core.Models;

/// <summary>
/// A capitalisation policy.
/// </summary>
public enum CapitalisationPolicy
{
    /// <summary>
    /// Capitalise the first letter of each word.
    /// </summary>
    TitleCase,

    /// <summary>
    /// Capitalise the first letter of the first word.
    /// </summary>
    SentenceCase,

    /// <summary>
    /// Convert all letters to lowercase.
    /// </summary>
    LowerCase,

    /// <summary>
    /// Convert all letters to uppercase.
    /// </summary>
    UpperCase
}
