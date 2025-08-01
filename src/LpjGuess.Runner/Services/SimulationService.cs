using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Models;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
public class SimulationService : ISimulationService
{
	/// <summary>
	/// The path resolver.
	/// </summary>
	private readonly IPathResolver pathResolver;

	/// <summary>
	/// The configuration to use.
	/// </summary>
	private readonly SimulationGeneratorConfig config;

	/// <summary>
	/// Create a new <see cref="SimulationService"/> instance.
	/// </summary>
	/// <param name="config">The configuration to use.</param>
	public SimulationService(SimulationGeneratorConfig config) : this(new SimulationPathResolver(config), config)
	{
	}

	/// <summary>
	/// Create a new <see cref="SimulationService"/> instance.
	/// </summary>
	/// <param name="pathResolver">The path resolver.</param>
	/// <param name="config">The configuration to use.</param>
	public SimulationService(IPathResolver pathResolver, SimulationGeneratorConfig config)
	{
		this.pathResolver = pathResolver;
		this.config = config;
	}

	/// <summary>
	/// Generate all jobs for these instructions.
	/// </summary>
	/// <param name="ct">Cancellation token.</param>
	public IEnumerable<Job> GenerateAllJobs(CancellationToken ct)
	{
		IEnumerable<string> query = config.InsFiles;
		if (config.Parallel)
		{
			query = query.AsParallel()
						 .WithDegreeOfParallelism(config.CpuCount)
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
		IEnumerable<ISimulation> query = config.Simulations;

		if (config.Parallel)
		{
			query = query.AsParallel()
			 			 .WithDegreeOfParallelism(config.CpuCount)
						 .WithCancellation(ct);
		}

		return query.Select(f => GenerateSimulation(insFile, f));
	}

	/// <summary>
	/// Generate the specified simulation for the given instruction file.
	/// </summary>
	/// <param name="insFile">Path to the instruction file.</param>
	/// <param name="simulation">The simulation.</param>
	/// <returns>A job encapsulating the generated simulation.</returns>
	private Job GenerateSimulation(string insFile, ISimulation simulation)
	{
		// Choose an instruction file name and path.
		string targetInsFile = pathResolver.GenerateTargetInsFilePath(insFile, simulation);

		// Create the output directory if it doesn't already exist.
		string? directory = Path.GetDirectoryName(targetInsFile);
		if (directory != null)
			Directory.CreateDirectory(directory);

		// Apply changes from this factorial.
		simulation.Generate(insFile, targetInsFile, config.Pfts);

		// Return a job object encapsulating this information.
		return new Job(simulation.Name, targetInsFile);
	}
}
