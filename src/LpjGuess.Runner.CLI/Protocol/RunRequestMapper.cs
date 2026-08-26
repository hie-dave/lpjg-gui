using System.Globalization;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Factorial;
using LpjGuess.Core.Models.Factorial.Factors;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Protocol;

public static class RunRequestMapper
{
    public static (RunnerConfiguration Configuration, ExistingOutputPolicy Policy) Map(
        RunRequest request)
    {
        if (request.ProtocolVersion != ProtocolVersion.Current)
            throw new ArgumentException(
                $"Unsupported protocol version {request.ProtocolVersion}; expected {ProtocolVersion.Current}.");
        if (request.InstructionFiles.Count == 0)
            throw new ArgumentException("At least one instruction file is required.");

        RunSettingsDto input = request.Settings;
        if (!TimeSpan.TryParseExact(input.Walltime, "c", CultureInfo.InvariantCulture,
                                    out TimeSpan walltime))
            throw new ArgumentException($"Invalid walltime '{input.Walltime}'; expected d.hh:mm:ss or hh:mm:ss.");

        ExistingOutputPolicy policy = ExistingOutputPolicyExtensions.ParseExistingOutputPolicy(
            request.ExistingOutputPolicy ?? "clean_managed");
        RunSettings settings = new(
            input.DryRun, input.RunLocal, input.OutputDirectory, input.GuessPath,
            input.InputModule, input.CpuCount, walltime, input.Memory,
            input.Queue, input.Project, input.EmailNotifications,
            input.EmailAddress, input.JobName, input.FullFactorial,
            input.UseCpuAffinity, policy);

        List<ISimulation> simulations = request.Simulations.Select(MapSimulation).ToList();
        return (new RunnerConfiguration(settings, simulations,
                                        request.InstructionFiles, request.Pfts),
                policy);
    }

    private static ISimulation MapSimulation(SimulationDto simulation)
    {
        if (string.IsNullOrWhiteSpace(simulation.Name))
            throw new ArgumentException("Simulation names may not be empty.");
        // Materialise here so malformed factors fail while validating the
        // request, rather than later during job generation.
        return new Simulation(simulation.Name,
                              simulation.Factors.Select(MapFactor).ToList());
    }

    private static IFactor MapFactor(FactorDto factor) => factor.Type switch
    {
        "top_level" => new TopLevelParameter(factor.Name, factor.Value),
        "block" when !string.IsNullOrWhiteSpace(factor.BlockType) &&
                     !string.IsNullOrWhiteSpace(factor.BlockName)
            => new BlockParameter(factor.BlockType, factor.BlockName,
                                  factor.Name, factor.Value),
        "block" => throw new ArgumentException(
            "Block factors require block_type and block_name."),
        _ => throw new ArgumentException($"Unsupported factor type '{factor.Type}'.")
    };
}
