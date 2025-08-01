using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
/// <remarks>
/// Refactor most of the logic out of here.
/// </remarks>
public class SimulationService : ISimulationService
{
	/// <summary>
	/// List of paths to instruction files to be run.
	/// </summary>
	public IReadOnlyCollection<string> InsFiles { get; private init; }

	/// <summary>
	/// List of PFTs to be enabled for this run. All others will be disabled.
	/// If empty, no PFTs will be disabled.
	/// </summary>
	public IReadOnlyCollection<string> Pfts { get; private init; }

	/// <summary>
	/// The parameter changes being applied in this run.
	/// </summary>
	public IReadOnlyCollection<ISimulation> Simulations { get; private init; }

	/// <summary>
	/// Create a new <see cref="SimulationService"/> instance.
	/// </summary>
	/// <param name="config">The configuration to use.</param>
	public SimulationService(SimulationGeneratorConfig config)
	{
		InsFiles = config.InsFiles;
		Pfts = config.Pfts;

		Simulations = config.Factors;
	}

	/// <summary>
	/// Generate all jobs for these instructions.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public IEnumerable<Job> GenerateAllJobs(CancellationToken ct)
	{
		IEnumerable<string> query = InsFiles;
		if (Settings.Parallel)
		{
			query = query.AsParallel()
						 .WithDegreeOfParallelism(Settings.CpuCount)
						 .WithCancellation(ct);
		}

		return query.SelectMany(ins => GenerateJobs(ins, ct));
	}

	/// <summary>
	/// Generate all jobs to be run for the specified instruction file.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	private IEnumerable<Job> GenerateJobs(string insFile, CancellationToken ct)
	{
		IEnumerable<ISimulation> query = Simulations;

		if (Settings.Parallel)
		{
			query = query.AsParallel()
			 			 .WithDegreeOfParallelism(Settings.CpuCount)
						 .WithCancellation(ct);
		}

		return query.Select(f => GenerateSimulation(insFile, f));
	}

	private Job GenerateSimulation(string insFile, ISimulation simulation)
	{
		// Apply changes from this factorial.
		string jobName = simulation.Name;
		string insName = Path.GetFileNameWithoutExtension(insFile);

		if (InsFiles.Count > 1)
			jobName = $"{insName}-{jobName}";

        string jobDirectory = Settings.OutputDirectory;
		if (InsFiles.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, insName);
		if (Simulations.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, simulation.Name);

		Directory.CreateDirectory(jobDirectory);
		string targetInsFile = Path.Combine(jobDirectory, $"{jobName}.ins");

		simulation.Generate(insFile, targetInsFile, Pfts);
		return new Job(jobName, targetInsFile);
	}
}
