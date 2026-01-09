using System.Text;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Dtos;
using LpjGuess.Runner.Models;
using Tomlyn;

namespace LpjGuess.Runner.Services;

/// <summary>
/// Default implementation of <see cref="IResultCatalog"/> writing simple TOML files.
/// </summary>
public class ResultCatalog : IResultCatalog
{
    private const string manifestFileName = "manifest.toml";
    private const string indexFileName = "index.toml";

    /// <inheritdoc/>
    public SimulationIndex ReadIndex(IPathResolver pathResolver)
    {
        Task<SimulationIndex> task = ReadIndexAsync(pathResolver);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc/>
    public SimulationManifest ReadManifest(string simulationDirectory)
    {
        Task<SimulationManifest> task = ReadManifestAsync(simulationDirectory);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc/>
    public void WriteIndex(IPathResolver pathResolver, SimulationIndex index)
    {
        WriteIndexAsync(pathResolver, index).Wait();
    }

    /// <inheritdoc/>
    public void WriteSimulation(SimulationManifest manifest)
    {
        WriteSimulationAsync(manifest).Wait();
    }

    /// <summary>
    /// Writes a simulation manifest to disk asynchronously.
    /// </summary>
    /// <param name="manifest">The manifest to write.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown if the simulation directory does not exist.</exception>
    public async Task WriteSimulationAsync(SimulationManifest manifest)
    {
        if (!Directory.Exists(manifest.Path))
            throw new DirectoryNotFoundException(manifest.Path);

        string manifestPath = Path.Combine(manifest.Path, manifestFileName);
        TomlModelOptions opts = GetSerialisationOptions();
        await File.WriteAllTextAsync(manifestPath, Toml.FromModel(manifest, opts));
    }

    /// <summary>
    /// Writes a simulation index to disk asynchronously.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="index">The index to write.</param>
    public async Task WriteIndexAsync(IPathResolver pathResolver, SimulationIndex index)
    {
        // Get the absolute path to the index file.
        string indexPath = pathResolver.GetAbsolutePath(indexFileName);

        // Write the index to disk.
        TomlModelOptions opts = GetSerialisationOptions();
        await File.WriteAllTextAsync(indexPath, Toml.FromModel(index, opts));
    }

    /// <summary>
    /// Reads a simulation manifest from disk asynchronously.
    /// </summary>
    /// <param name="simulationDirectory">The directory containing the simulation artifacts.</param>
    /// <returns>A <see cref="SimulationManifest"/> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the manifest file does not exist.</exception>
    public async Task<SimulationManifest> ReadManifestAsync(string simulationDirectory)
    {
        string manifestPath = Path.Combine(simulationDirectory, manifestFileName);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException(manifestPath);

        string content = await File.ReadAllTextAsync(manifestPath);
        TomlModelOptions opts = GetSerialisationOptions();
        return Toml.ToModel<SimulationManifestDto>(content, manifestPath, opts)
                   .ToSimulationManifest();
    }

    /// <summary>
    /// Reads a simulation index from disk asynchronously.
    /// </summary>
    /// <param name="pathResolver">The path resolver.</param>
    /// <returns>A <see cref="SimulationIndex"/> instance.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<SimulationIndex> ReadIndexAsync(IPathResolver pathResolver)
    {
        string indexPath = pathResolver.GetRelativePath(indexFileName);
        if (!File.Exists(indexPath))
            throw new FileNotFoundException(indexPath);

        string content = await File.ReadAllTextAsync(indexPath);
        TomlModelOptions opts = GetSerialisationOptions();
        return Toml.ToModel<SimulationIndexDto>(content, indexPath, opts)
                   .ToSimulationIndex();
    }

    /// <summary>
    /// Gets the serialisation options for TOML serialisation.
    /// </summary>
    /// <returns>The serialisation options.</returns>
    private static TomlModelOptions GetSerialisationOptions()
    {
        return new TomlModelOptions();
    }
}
