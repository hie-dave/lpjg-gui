
using System.Diagnostics;
using System.Globalization;

namespace LpjGuess.Runner.Models;

/// <summary>
/// A class which runs LPJ-Guess on gadi via PBS.
/// </summary>
public class PbsRunner : IRunner
{
    /// <summary>
    /// Name of the lpj-guess submission script.
    /// </summary>
    private const string submitScript = "submit_to_gadi.sh";

	/// <summary>
	/// The input module to use.
	/// </summary>
	private readonly string inputModule;

    /// <summary>
    /// Settings to be used for the run.
    /// </summary>
    private readonly PbsRunnerConfiguration settings;

    /// <summary>
    /// Create a new <see cref="PbsRunner"/> instance.
    /// </summary>
	/// <param name="settings">Runner settings.</param>
	/// <param name="inputModule">The input module to use.</param>
    public PbsRunner(PbsRunnerConfiguration settings, string inputModule)
    {
        this.settings = settings;
		this.inputModule = inputModule;
    }

    /// <inheritdoc />
    public async Task RunAsync(Job job, CancellationToken ct)
    {
		try
		{
			string confFile = await GenerateConfFile(job.InsFile, job.Name);
			ct.ThrowIfCancellationRequested();

			using (Process proc = new Process())
			{
				proc.StartInfo.FileName = submitScript;
				proc.StartInfo.ArgumentList.Add("-q");
				proc.StartInfo.ArgumentList.Add("-s");
				proc.StartInfo.ArgumentList.Add(confFile);
				if (settings.DryRun)
					proc.StartInfo.ArgumentList.Add("-d");

				// Run the submit script.
				proc.Start();
				await proc.WaitForExitAsync(ct);
				if (ct.IsCancellationRequested && !proc.HasExited)
					proc.Kill();
				ct.ThrowIfCancellationRequested();
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to run submit script for .ins file '{job.InsFile}'", error);
		}
    }

	/// <summary>
	/// Generate the config file which may be passed to the submit script.
	/// </summary>
	/// <param name="insFile">Path to the .ins file.</param>
	/// <param name="factName">A name which uniquely identifies this factorial.</param>
	private async Task<string> GenerateConfFile(
		string insFile,
		string factName)
	{
		try
		{
			// Key names.
			const string binary = "BINARY";
			const string nprocess = "NPROCESS";
			const string walltime = "WALLTIME";
			const string memory = "MEMORY";
			const string queue = "QUEUE";
			const string project = "PROJECT";
			const string email = "EMAIL";
			const string emailNotifications = "EMAIL_NOTIFICATIONS";
			const string jobName = "JOB_NAME";
			const string outDir = "OUT_DIR";
			const string insfile = "INSFILE";
			const string inputModule = "INPUT_MODULE";		
			const string experiment = "EXPERIMENT";

			string name = Path.GetFileNameWithoutExtension(insFile);
			string confFile = Path.Combine(Path.GetTempPath(), $"{name}.conf");

			// This is the OUTPUT_DIR we pass to the submit script.
			string outPath = Path.Combine(settings.OutputDirectory);
			if (!Directory.Exists(outPath))
				Directory.CreateDirectory(outPath);

			// This is the actual directory the submit script will use for this
			// particular job. If it already exists, that means the user has
			// probably accidentally re-used the same directory. We don't want
			// to overwrite files from a previous run, so let's force them to be
			// explcit about what they want.
			string actualOutDir = Path.Combine(outPath, factName);

			using (Stream stream = File.Open(confFile, FileMode.Create))
			using (TextWriter writer = new StreamWriter(stream))
			{
				await Write(writer, binary, settings.GuessPath);
				await Write(writer, nprocess, settings.CpuCount);
				await Write(writer, walltime, settings.Walltime);
				await Write(writer, memory, $"{settings.Memory}GB");
				await Write(writer, queue, settings.Queue);
				await Write(writer, project, settings.Project);
				await Write(writer, email, settings.EmailAddress ?? string.Empty);
				await Write(writer, emailNotifications, settings.EmailNotifications ? "1" : "0");
				await Write(writer, jobName, factName);
				await Write(writer, outDir, outPath);
				await Write(writer, insfile, insFile);
				await Write(writer, inputModule, this.inputModule);
				await Write(writer, experiment, factName);
			}
			return confFile;
		}
		catch (Exception error)
		{
			throw new Exception($"Failed to generate .conf file for .ins file '{insFile}'", error);
		}
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, TimeSpan value)
	{
		string s = $"{value.Hours:D2}:{value.Minutes:D2}:{value.Seconds:D2}";
		await writer.WriteLineAsync($"{key}={s}");
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, uint value)
	{
		string str = value.ToString(CultureInfo.InvariantCulture);
		await writer.WriteLineAsync($"{key}={str}");
	}

	/// <summary>
	/// Write a shell-quoted integer key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, int value)
	{
		string str = value.ToString(CultureInfo.InvariantCulture);
		await writer.WriteLineAsync($"{key}={str}");
	}

	/// <summary>
	/// Write a shell-quoted key-value pair to the output .conf file.
	/// </summary>
	/// <param name="writer">The output text writer.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Value.</param>
	private async Task Write(TextWriter writer, string key, string value)
	{
		await writer.WriteLineAsync($"{key}=\"{value}\"");
	}
}
