using System.Diagnostics;
using System.Globalization;
using LpjGuess.Runner.Parsers;

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
	public IReadOnlyCollection<Factorial> Factorials { get; private init; }

	/// <summary>
	/// Run settings.
	/// </summary>
	public RunSettings Settings { get; private init; }

	/// <summary>
	/// Create a new <see cref="SimulationGenerator"/> instance.
	/// </summary>
	/// <param name="insFiles">List of paths to instruction files to be run.</param>
	/// <param name="pfts">List of PFTs to be enabled for this run. All others will be disabled. If empty, no PFTs will be disabled.</param>
	/// <param name="parameters">The parameter changes being applied in this run.</param>
	/// <param name="settings">Run settings.</param>
	public SimulationGenerator(IReadOnlyCollection<string> insFiles, IReadOnlyCollection<string> pfts
		, IReadOnlyCollection<Factorial> factorials, RunSettings settings)
	{
		InsFiles = insFiles;
		Pfts = pfts;
		Settings = settings;
		if (factorials.Count == 0)
			factorials = new List<Factorial>() { new Factorial(new List<Factor>()) };
		Factorials = factorials;
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
		IEnumerable<Factorial> query = Factorials;

		if (Settings.Parallel)
		{
			query = query.AsParallel()
			 			 .WithDegreeOfParallelism(Settings.CpuCount)
						 .WithCancellation(ct);
		}

		return query.Select(f => GenerateSimulation(insFile, f));
	}

	private Job GenerateSimulation(string insFile, Factorial factorial)
	{
		// Apply changes from this factorial.
		string jobName = factorial.GetName();
		if (InsFiles.Count > 1)
			jobName = $"{Path.GetFileNameWithoutExtension(insFile)}-{jobName}";
		string targetInsFile = ApplyOverrides(factorial, insFile, jobName);
		return new Job(jobName, targetInsFile);
	}

	private string ApplyOverrides(Factorial factorial, string insFile, string name)
	{
		string file = Path.GetFileNameWithoutExtension(insFile);
		string ext = Path.GetExtension(insFile);
		string jobDirectory = Settings.OutputDirectory;
		if (insFile.Length > 1 && Factorials.Count > 1)
			jobDirectory = Path.Combine(jobDirectory, file);
		jobDirectory = Path.Combine(jobDirectory, name);
		string targetInsFile = Path.Combine(jobDirectory, $"{file}-{name}{ext}");
		Directory.CreateDirectory(jobDirectory);

		try
		{
			InstructionFileParser ins = InstructionFileParser.FromFile(insFile);

			foreach (Factor factor in factorial.Factors)
				ins.ApplyChange(factor);

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

	private void CleanupFailure(Exception error, string file)
	{
		Console.Error.WriteLine($"WARNING: Failed to clean temporary file: '{file}':");
		Console.Error.WriteLine(error);
	}
}
