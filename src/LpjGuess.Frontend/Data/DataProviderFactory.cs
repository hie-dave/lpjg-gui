using LpjGuess.Core.Interfaces;
using LpjGuess.Core.Models;
using LpjGuess.Frontend.Data.Providers;
using LpjGuess.Frontend.DependencyInjection;
using LpjGuess.Runner.Services;

namespace LpjGuess.Frontend.Data;

/// <summary>
/// Factory for creating data providers.
/// </summary>
public class DataProviderFactory : IDataProviderFactory
{
    /// <summary>
    /// The instruction files provider.
    /// </summary>
    private readonly IInstructionFilesProvider insFilesProvider;

    /// <summary>
    /// The experiments provider.
    /// </summary>
    private readonly IExperimentProvider experimentsProvider;

    /// <summary>
    /// The path resolver.
    /// </summary>
    private readonly IPathResolver resolver;

    /// <summary>
    /// Create a new <see cref="DataProviderFactory"/> instance.
    /// </summary>
    /// <param name="insFilesProvider">The instruction files provider.</param>
    /// <param name="experimentsProvider">The experiments provider.</param>
    /// <param name="resolver">The path resolver.</param>
    public DataProviderFactory(
        IInstructionFilesProvider insFilesProvider,
        IExperimentProvider experimentsProvider,
        IPathResolver resolver)
    {
        this.insFilesProvider = insFilesProvider;
        this.experimentsProvider = experimentsProvider;
        this.resolver = resolver;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SeriesData>> ReadAsync(IDataSource source, CancellationToken ct)
    {
        if (source is ModelOutput modelOutput)
        {
            return await new ModelOutputReader(insFilesProvider, experimentsProvider, resolver).ReadAsync(modelOutput, ct);
        }

        throw new NotSupportedException($"Data provider not supported for {typeof(IDataSource).Name}");
    }

    /// <inheritdoc />
    public string GetName(IDataSource source)
    {
        // fixme!!
        if (source is ModelOutput modelOutput)
            return new ModelOutputReader(insFilesProvider, experimentsProvider, resolver).GetName(modelOutput);

        throw new NotSupportedException($"Data provider not supported for {typeof(IDataSource).Name}");
    }
}
