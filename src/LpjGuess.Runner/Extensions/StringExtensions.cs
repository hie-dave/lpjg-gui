namespace LpjGuess.Runner.Extensions;

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
}
