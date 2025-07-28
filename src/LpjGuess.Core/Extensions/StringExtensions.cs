using LpjGuess.Core.Models;

namespace LpjGuess.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="string"/> objects.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Splits a string into substrings, honouring quotes.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <param name="separators">The characters to split on.</param>
    /// <param name="allowSingleQuotes">Whether to allow single quotes as quotes.</param>
    /// <remarks>
    /// Examples:
    /// 
    /// a,b,c -> ["a", "b", "c"]
    /// a,"b, c",d -> ["a", "b, c", "d"]
    /// </remarks>
    /// <returns>The substrings.</returns>
    public static string[] SplitHonouringQuotes(this string str, char[] separators, bool allowSingleQuotes = false)
    {
        List<char> quotes = [ '"' ];
        if (allowSingleQuotes)
            quotes.Add('\'');

        // Parse string.
        bool insideQuotes = false;
        List<string> result = new List<string>();
        int substringStart = 0;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (quotes.Contains(c))
                insideQuotes = !insideQuotes;
            else if (separators.Contains(c) && !insideQuotes)
            {
                result.Add(str.Substring(substringStart, i - substringStart));
                substringStart = i + 1;
            }
        }

        // Add last substring
        result.Add(str.Substring(substringStart));
        return result.ToArray();
    }

    /// <summary>
    /// Convert a PascalCase string to a human-readable Pascal Case string.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <param name="capitalisationPolicy">The capitalisation policy to use.</param>
    /// <returns>The human-readable string.</returns>
    public static string PascalToHumanCase(this string str, CapitalisationPolicy capitalisationPolicy = CapitalisationPolicy.TitleCase)
    {
        return string.Join(" ", str.SplitCamelCase(capitalisationPolicy));
    }

    /// <summary>
    /// Split a PascalCase string into an array of words.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <param name="capitalisationPolicy">The capitalisation policy to use.</param>
    /// <returns>The array of words.</returns>
    private static string[] SplitCamelCase(this string str, CapitalisationPolicy capitalisationPolicy = CapitalisationPolicy.TitleCase)
    {
        List<string> words = new List<string>();
        int start = 0;
        for (int i = 1; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]))
            {
                string word = str[start..i];
                word = ConditionallyCapitalise(word, capitalisationPolicy, start == 0);
                words.Add(word);
                start = i;
            }
        }
        words.Add(str[start..]);
        return words.ToArray();
    }

    /// <summary>
    /// Conditionally capitalise a word based on the capitalisation policy.
    /// </summary>
    /// <param name="word">The word to capitalise.</param>
    /// <param name="capitalisationPolicy">The capitalisation policy to use.</param>
    /// <param name="isFirstWord">Whether the word is the first word in the string.</param>
    /// <returns>The capitalised word.</returns>
    private static string ConditionallyCapitalise(string word, CapitalisationPolicy capitalisationPolicy, bool isFirstWord)
    {
        switch (capitalisationPolicy)
        {
            case CapitalisationPolicy.TitleCase:
                return char.ToUpper(word[0]) + word[1..];
            case CapitalisationPolicy.SentenceCase:
                return isFirstWord ? char.ToUpper(word[0]) + word[1..] : word;
            case CapitalisationPolicy.LowerCase:
                return word.ToLower();
            case CapitalisationPolicy.UpperCase:
                return word.ToUpper();
            default:
                throw new ArgumentException($"Invalid capitalisation policy: {capitalisationPolicy}");
        }
    }
}
