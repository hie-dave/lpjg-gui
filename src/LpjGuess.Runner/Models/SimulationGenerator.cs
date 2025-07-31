using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Runner.Models;

/// <summary>
/// Changes to be applied to a collection of LPJ-Guess instruction files.
/// </summary>
/// <remarks>
/// Refactor most of the logic out of here.
/// </remarks>
public class SimulationGenerator
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
	public IReadOnlyCollection<IFactors> Simulations { get; private init; }

	/// <summary>
	/// Run settings.
	/// </summary>
	public RunSettings Settings { get; private init; }

	/// <summary>
	/// Create a new <see cref="SimulationGenerator"/> instance.
	/// </summary>
	/// <param name="config">The configuration to use.</param>
	public SimulationGenerator(RunnerConfiguration config)
	{
		InsFiles = config.InsFiles;
		Pfts = config.Pfts;
		Settings = config.Settings;

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
		IEnumerable<IFactors> query = Simulations;

		if (Settings.Parallel)
		{
			query = query.AsParallel()
			 			 .WithDegreeOfParallelism(Settings.CpuCount)
						 .WithCancellation(ct);
		}

		return query.Select(f => GenerateSimulation(insFile, f));
	}

	private Job GenerateSimulation(string insFile, IFactors factors)
	{
		// Apply changes from this factorial.
		string jobName = factors.Name;
		if (InsFiles.Count > 1)
			jobName = $"{Path.GetFileNameWithoutExtension(insFile)}-{jobName}";
		string targetInsFile = ApplyOverrides(factors, insFile, jobName);
		return new Job(jobName, targetInsFile);
	}

	private string ApplyOverrides(IFactors factors, string insFile, string name)
	{
		string file = Path.GetFileNameWithoutExtension(insFile);
		string ext = Path.GetExtension(insFile);
		string jobDirectory = Settings.OutputDirectory;
		if (insFile.Length > 1 && Simulations.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, file);
		jobDirectory = Path.Combine(jobDirectory, name);
		string targetInsFile = Path.Combine(jobDirectory, $"{file}-{name}{ext}");
		Directory.CreateDirectory(jobDirectory);

		try
		{
			InstructionFileParser ins = InstructionFileParser.FromFile(insFile);

			foreach (IFactor factor in factors.Changes)
				factor.Apply(ins);

			// Disable all PFTs except those required.
			if (Pfts.Count > 0)
			{
				ins.DisableAllPfts();
				foreach (string pft in Pfts)
					ins.EnablePft(pft);
			}

			string content = ins.GenerateContent();
			File.WriteAllText(targetInsFile, content);

			return targetInsFile;
		}
		catch (Exception error)
		{
			try
			{
				if (File.Exists(targetInsFile))
					File.Delete(targetInsFile);
			}
			catch (Exception nested)
			{
				CleanupFailure(nested, targetInsFile);
			}
			throw new Exception($"Failed to apply overrides to file '{insFile}'", error);
		}
	}

	private static void CleanupFailure(Exception error, string file)
	{
		Console.Error.WriteLine($"WARNING: Failed to clean temporary file: '{file}':");
		Console.Error.WriteLine(error);
	}
}
