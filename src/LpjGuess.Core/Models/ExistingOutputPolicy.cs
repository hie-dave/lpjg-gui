using System.Runtime.Serialization;

namespace LpjGuess.Core.Models;

/// <summary>
/// Policies controlling how existing outputs should be handled.
/// </summary>
[Flags]
public enum ExistingOutputPolicy
{
    /// <summary>
    /// Keep any existing outputs from previous runs.
    /// </summary>
    [EnumMember(Value = "preserve")]
    Preserve = 0,

    /// <summary>
    /// Delete any existing outputs from previous runs which will not be
    /// overwritten by the current run.
    /// </summary>
    /// <remarks>
    /// This ensures that old simulations which are not being rerun are not
    /// retained.
    /// </remarks>
    [EnumMember(Value = "prune_stale")]
    PruneStale = 1,

    /// <summary>
    /// Delete any existing outputs from previous runs which will be overwritten
    /// by the current run.
    /// </summary>
    /// <remarks>
    /// This prevents obsolete output files remaining inside simulations that
    /// are being rerun.
    /// </remarks>
    [EnumMember(Value = "clean_managed")]
    CleanManaged = 2,

    /// <summary>
    /// Fail if any existing outputs are found.
    /// </summary>
    [EnumMember(Value = "fail")]
    Fail = 4
}

/// <summary>
/// Extension methods for <see cref="ExistingOutputPolicy"/>.
/// </summary>
public static class ExistingOutputPolicyExtensions
{
    /// <summary>
    /// Parses a string into an <see cref="ExistingOutputPolicy"/>.
    /// </summary>
    /// <param name="policy">The string to parse. Multiple flags may be
    /// separated by commas.</param>
    /// <returns>The parsed policy.</returns>
    /// <exception cref="ArgumentException">Thrown if the input string does not
    /// match any policy.</exception>
    public static ExistingOutputPolicy ParseExistingOutputPolicy(string policy)
    {
        ExistingOutputPolicy result = ExistingOutputPolicy.Preserve;
        foreach (string part in policy.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            ExistingOutputPolicy parsed = part.ToLowerInvariant() switch
            {
                "preserve" => ExistingOutputPolicy.Preserve,
                "prune_stale" => ExistingOutputPolicy.PruneStale,
                "clean_managed" => ExistingOutputPolicy.CleanManaged,
                "fail" => ExistingOutputPolicy.Fail,
                _ => throw new ArgumentException(
                    $"Invalid existing output policy: {policy}")
            };
            result |= parsed;
        }
        return result;
    }

    /// <summary>
    /// Convert an <see cref="ExistingOutputPolicy"/> into its serialised form.
    /// </summary>
    /// <param name="policy">The policy to serialise.</param>
    /// <returns>The serialised policy.</returns>
    public static string ToConfigString(this ExistingOutputPolicy policy)
    {
        if (policy == ExistingOutputPolicy.Preserve)
            return "preserve";

        List<string> parts = [];
        if ((policy & ExistingOutputPolicy.PruneStale) == ExistingOutputPolicy.PruneStale)
            parts.Add("prune_stale");
        if ((policy & ExistingOutputPolicy.CleanManaged) == ExistingOutputPolicy.CleanManaged)
            parts.Add("clean_managed");
        if ((policy & ExistingOutputPolicy.Fail) == ExistingOutputPolicy.Fail)
            parts.Add("fail");
        return string.Join(",", parts);
    }
}
