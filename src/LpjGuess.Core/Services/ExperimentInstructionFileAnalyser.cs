using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Generators;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Core.Services;

/// <summary>
/// Validates an experiment design against its selected instruction files.
/// </summary>
public static class ExperimentInstructionFileAnalyser
{
    /// <summary>
    /// Validate structural targets which can be proven locally. Parameter names
    /// which are absent are informational because they may be optional or
    /// inherited.
    /// </summary>
    public static IReadOnlyList<ExperimentDesignIssue> Analyse(
        FactorialGenerator generator,
        IEnumerable<string> instructionFiles,
        IEnumerable<string> pfts)
    {
        List<ExperimentDesignIssue> issues = [];
        List<ParameterTarget> targets = generator.Factors
            .SelectMany(factor => factor.GetParameterTargets())
            .Distinct()
            .ToList();
        List<string> selectedPfts = pfts.Distinct().ToList();

        foreach (string file in instructionFiles)
        {
            string fileName = Path.GetFileName(file);
            if (!File.Exists(file))
            {
                issues.Add(Error($"Instruction file '{fileName}' does not exist."));
                continue;
            }

            InstructionFileParser parser;
            try
            {
                parser = InstructionFileParser.FromFile(file);
            }
            catch (Exception error)
            {
                issues.Add(Error(
                    $"Instruction file '{fileName}' could not be parsed: {error.Message}"));
                continue;
            }

            foreach (ParameterTarget target in targets)
            {
                if (target.BlockType is null)
                {
                    if (parser.GetTopLevelParameter(target.ParameterName) is null)
                        issues.Add(Information(
                            $"'{target.DisplayName}' is not explicitly defined in '{fileName}'. " +
                            "It may be an optional parameter."));
                    continue;
                }

                string blockName = target.BlockName ?? string.Empty;
                bool blockExists = parser.GetBlocks()
                    .Contains((target.BlockType, blockName));
                if (!blockExists)
                {
                    issues.Add(Error(
                        $"Block '{target.BlockType}[{blockName}]' is not defined in '{fileName}'."));
                    continue;
                }

                if (parser.GetBlockParameter(
                    target.BlockType,
                    blockName,
                    target.ParameterName) is null)
                    issues.Add(Information(
                        $"'{target.DisplayName}' is not explicitly defined in '{fileName}'. " +
                        "It may be inherited or optional."));
            }

            foreach (string pft in selectedPfts.Where(pft => !parser.IsPft(pft)))
                issues.Add(Error($"PFT '{pft}' is not defined in '{fileName}'."));
        }

        return issues;
    }

    private static ExperimentDesignIssue Error(string message)
        => new(ExperimentDesignIssueSeverity.Error, message);

    private static ExperimentDesignIssue Information(string message)
        => new(ExperimentDesignIssueSeverity.Information, message);
}
