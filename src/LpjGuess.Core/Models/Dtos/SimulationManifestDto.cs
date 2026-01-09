namespace LpjGuess.Core.Models.Dtos;

/// <summary>
/// DTO for serialisation of <see cref="SimulationManifest"/>.
/// </summary>
public class SimulationManifestDto
{
    /// <summary>
    /// The unique key for the simulation.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The name of the simulation.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The path to the simulation directory.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The path to the base instruction file.
    /// </summary>
    public string BaseIns { get; set; }

    /// <summary>
    /// The path to the generated instruction file.
    /// </summary>
    public string InsFile { get; set; }

    /// <summary>
    /// The list of PFTs used in the simulation.
    /// </summary>
    public List<string> Pfts { get; set; }

    /// <summary>
    /// The list of factors used in the simulation.
    /// </summary>
    public List<FactorDto> Factors { get; set; }

    /// <summary>
    /// The date and time the simulation was generated.
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; }

    /// <summary>
    /// Creates a default <see cref="SimulationManifestDto"/>.
    /// </summary>
    public SimulationManifestDto()
    {
        Key = string.Empty;
        Name = string.Empty;
        Path = string.Empty;
        BaseIns = string.Empty;
        InsFile = string.Empty;
        Pfts = [];
        Factors = [];
        GeneratedAtUtc = DateTime.MinValue;
    }

    /// <summary>
    /// Creates a <see cref="SimulationManifestDto"/> from a
    /// <see cref="SimulationManifest"/>.
    /// </summary>
    public static SimulationManifestDto FromSimulationManifest(
        SimulationManifest manifest)
    {
        return new SimulationManifestDto
        {
            Key = manifest.Key,
            Name = manifest.Name,
            Path = manifest.Path,
            BaseIns = manifest.BaseIns,
            InsFile = manifest.InsFile,
            Pfts = manifest.Pfts.ToList(),
            Factors = manifest.Factors.Select(FactorDto.FromFactor).ToList(),
            GeneratedAtUtc = manifest.GeneratedAtUtc
        };
    }

    /// <summary>
    /// Creates a <see cref="SimulationManifest"/> from a
    /// <see cref="SimulationManifestDto"/>.
    /// </summary>
    public SimulationManifest ToSimulationManifest()
    {
        return new SimulationManifest(
            Key,
            Name,
            Path,
            BaseIns,
            InsFile,
            Pfts,
            Factors.Select(f => f.ToFactor()).ToList(),
            GeneratedAtUtc);
    }
}
