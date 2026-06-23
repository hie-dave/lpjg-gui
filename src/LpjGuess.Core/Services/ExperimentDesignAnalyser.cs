using LpjGuess.Core.Extensions;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Models.Factorial.Generators.Factors;
using LpjGuess.Core.Models.Factorial.Generators.Values;

namespace LpjGuess.Core.Services;

/// <summary>
/// Calculates experiment size and reports incomplete or conflicting designs.
/// </summary>
public static class ExperimentDesignAnalyser
{
    /// <summary>
    /// Analyse a factorial experiment without materialising its simulations.
    /// </summary>
    /// <param name="generator">The experiment design.</param>
    /// <param name="instructionFileCount">Number of selected base instruction files.</param>
    public static ExperimentDesignAnalysis Analyse(
        FactorialGenerator generator,
        int instructionFileCount)
    {
        List<IFactorGenerator> factors = generator.Factors.ToList();
        List<ExperimentDesignIssue> issues = [];

        if (instructionFileCount <= 0)
            issues.Add(Error("Select at least one base instruction file."));

        if (factors.Count == 0)
            issues.Add(Error("Add at least one parameter variation or scenario set."));

        foreach (IFactorGenerator factor in factors)
            ValidateFactor(factor, issues);

        if (generator.FullFactorial)
            ValidateCrossVariationConflicts(factors, issues);

        (long simulations, bool simulationOverflow) = CalculateSimulationCount(generator, factors);
        (long runs, bool runOverflow) = SaturatingMultiply(simulations, Math.Max(0, instructionFileCount));

        if (simulations > 10_000)
            issues.Add(Warning($"This design creates {simulations:N0} simulations. Preview is limited."));

        return new ExperimentDesignAnalysis(
            simulations,
            runs,
            simulationOverflow || runOverflow,
            issues);
    }

    private static void ValidateFactor(
        IFactorGenerator factor,
        ICollection<ExperimentDesignIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(factor.Name))
            issues.Add(Error("Every variation or scenario set needs a name."));

        int levelCount = factor.NumFactors();
        if (levelCount <= 0)
            issues.Add(Error($"'{DisplayName(factor)}' has no values or scenarios."));

        switch (factor)
        {
            case BlockFactorGenerator block:
                if (string.IsNullOrWhiteSpace(block.BlockType))
                    issues.Add(Error($"'{DisplayName(factor)}' needs a block type."));
                if (string.IsNullOrWhiteSpace(block.BlockName))
                    issues.Add(Error($"'{DisplayName(factor)}' needs a block name."));
                ValidateParameterName(block.Name, factor, issues);
                ValidateValues(block.Values, factor, issues);
                break;

            case TopLevelFactorGenerator parameter:
                ValidateParameterName(parameter.Name, factor, issues);
                ValidateValues(parameter.Values, factor, issues);
                break;

            case SimpleFactorGenerator scenarios:
                List<IFactor> levels = scenarios.Levels.ToList();
                foreach (IFactor level in levels)
                    ValidateScenario(level, factor, issues);
                foreach (string duplicateName in levels
                    .Select(level => level.GetName())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .GroupBy(name => name)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key))
                    issues.Add(Error(
                        $"'{DisplayName(factor)}' contains more than one scenario named '{duplicateName}'."));
                break;
        }
    }

    private static void ValidateParameterName(
        string parameterName,
        IFactorGenerator factor,
        ICollection<ExperimentDesignIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            issues.Add(Error($"'{DisplayName(factor)}' needs a parameter name."));
    }

    private static void ValidateValues(
        IValueGenerator values,
        IFactorGenerator factor,
        ICollection<ExperimentDesignIssue> issues)
    {
        if (values is IRangeGenerator range && range.N <= 0)
            issues.Add(Error($"'{DisplayName(factor)}' must generate at least one value."));

        if (values is DiscreteValues<string> strings && strings.Values.Any(string.IsNullOrWhiteSpace))
            issues.Add(Error($"'{DisplayName(factor)}' contains an empty value."));
    }

    private static void ValidateScenario(
        IFactor scenario,
        IFactorGenerator owner,
        ICollection<ExperimentDesignIssue> issues)
    {
        List<ParameterOverride> changes = scenario.GetParameterOverrides().ToList();
        if (scenario is CompositeFactor && changes.Count == 0)
            issues.Add(Error($"'{DisplayName(owner)}' contains an empty scenario."));

        if (string.IsNullOrWhiteSpace(scenario.GetName()))
            issues.Add(Error($"'{DisplayName(owner)}' contains an unnamed scenario."));

        foreach (ParameterOverride change in changes)
        {
            if (string.IsNullOrWhiteSpace(change.Target.ParameterName))
                issues.Add(Error($"A scenario in '{DisplayName(owner)}' has an empty parameter name."));
            if (change.Target.BlockType is not null &&
                (string.IsNullOrWhiteSpace(change.Target.BlockType) ||
                 string.IsNullOrWhiteSpace(change.Target.BlockName)))
                issues.Add(Error($"A scenario in '{DisplayName(owner)}' has an incomplete block target."));
            if (string.IsNullOrWhiteSpace(change.Value))
                issues.Add(Error(
                    $"A scenario in '{DisplayName(owner)}' has no value for '{change.Target.DisplayName}'."));
        }

        IEnumerable<string> duplicates = changes
            .GroupBy(c => c.Target)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.DisplayName);

        foreach (string target in duplicates)
            issues.Add(Error(
                $"A scenario in '{DisplayName(owner)}' changes '{target}' more than once."));
    }

    private static void ValidateCrossVariationConflicts(
        IEnumerable<IFactorGenerator> factors,
        ICollection<ExperimentDesignIssue> issues)
    {
        IEnumerable<IGrouping<ParameterTarget, IFactorGenerator>> duplicateTargets = factors
            .SelectMany(factor => factor.GetParameterTargets().Select(target => (factor, target)))
            .GroupBy(item => item.target, item => item.factor)
            .Where(group => group.Distinct().Count() > 1);

        foreach (IGrouping<ParameterTarget, IFactorGenerator> duplicate in duplicateTargets)
            issues.Add(Error(
                $"Combined variations change '{duplicate.Key.DisplayName}' more than once."));
    }

    private static (long Count, bool Overflowed) CalculateSimulationCount(
        FactorialGenerator generator,
        IReadOnlyCollection<IFactorGenerator> factors)
    {
        if (factors.Count == 0)
            return (0, false);

        long count = generator.FullFactorial ? 1 : 0;
        bool overflowed = false;

        foreach (IFactorGenerator factor in factors)
        {
            int levels = Math.Max(0, factor.NumFactors());
            (count, bool overflow) = generator.FullFactorial
                ? SaturatingMultiply(count, levels)
                : SaturatingAdd(count, levels);
            overflowed |= overflow;
        }

        return (count, overflowed);
    }

    private static (long Value, bool Overflowed) SaturatingAdd(long left, long right)
    {
        if (right > long.MaxValue - left)
            return (long.MaxValue, true);
        return (left + right, false);
    }

    private static (long Value, bool Overflowed) SaturatingMultiply(long left, long right)
    {
        if (left == 0 || right == 0)
            return (0, false);
        if (left > long.MaxValue / right)
            return (long.MaxValue, true);
        return (left * right, false);
    }

    private static string DisplayName(IFactorGenerator factor)
        => string.IsNullOrWhiteSpace(factor.Name) ? "Unnamed variation" : factor.Name;

    private static ExperimentDesignIssue Error(string message)
        => new(ExperimentDesignIssueSeverity.Error, message);

    private static ExperimentDesignIssue Warning(string message)
        => new(ExperimentDesignIssueSeverity.Warning, message);
}
