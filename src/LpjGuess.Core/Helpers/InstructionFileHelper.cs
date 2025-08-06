using LpjGuess.Core.Models;
using LpjGuess.Core.Parsers;
using Microsoft.Extensions.Logging;

namespace LpjGuess.Core.Helpers;

/// <summary>
/// Helper methods for parsing LPJ-GUESS instruction files.
/// </summary>
public class InstructionFileHelper
{
    /// <summary>
    /// Name of the instruction file parameter specifying the output directory.
    /// </summary>
    private const string outputDirectoryParameter = "outputdirectory";

    /// <summary>
    /// Name of the parameter that specifies the gridlist file.
    /// </summary>
    private const string paramGridlist = "file_gridlist";

    /// <summary>
    /// Name of the parameter that specifies the gridlist file when using the
    /// CF input module.
    /// </summary>
    private const string paramGridlistCf = "file_gridlist_cf";

    /// <summary>
    /// Name of the "str" parameter".
    /// </summary>
    private const string strBlock = "str";

    /// <summary>
    /// Name of the param block type.
    /// </summary>
    private const string parameterBlock = "param";

    /// <summary>
    /// Name of the stand block type.
    /// </summary>
    private const string standBlock = "st";

    /// <summary>
    /// Name of the PFT block type.
    /// </summary>
    private const string pftBlock = "pft";

    /// <summary>
    /// Name of the parameter that specifies whether a stand is included in the
    /// simulation.
    /// </summary>
    private const string standIncludeParameter = "stinclude";

    /// <summary>
    /// Name of the parameter that specifies whether a PFT is included in the
    /// simulation.
    /// </summary>
    private const string pftIncludeParameter = "include";

    /// <summary>
    /// Name of the instruction file parameter specifying the number of patches.
    /// </summary>
    private const string npatchParameter = "npatch";

    /// <summary>
    /// The defined landcover types.
    /// </summary>
    private static readonly string[] landcoverTypes = ["urban", "crop", "forest", "pasture", "natural", "peatland", "barren"];

    /// <summary>
    /// The parser used to parse the instruction file.
    /// </summary>
    private readonly InstructionFileParser parser;

    /// <summary>
    /// The logger used to log messages.
    /// </summary>
    private readonly ILogger<InstructionFileHelper> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionFileHelper"/> class.
    /// </summary>
    /// <param name="parser">The parser used to parse the instruction file.</param>
    /// <param name="logger">The logger used to log messages.</param>
    public InstructionFileHelper(InstructionFileParser parser, ILogger<InstructionFileHelper> logger)
    {
        this.parser = parser;
        this.logger = logger;
    }

    /// <summary>
    /// Get the gridlist parameter from an instruction file.
    /// </summary>
    /// <returns>Absolute path to the gridlist file.</returns>
    public string GetGridlist()
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(paramGridlist);
        if (parameter != null)
            return NormalisePath(parameter.AsString());
        parameter = parser.GetBlockParameter(parameterBlock, paramGridlistCf, strBlock);
        if (parameter is null)
            throw new InvalidOperationException($"Instruction file {parser.FilePath} does not contain a gridlist parameter");
        return NormalisePath(parameter.AsString());
    }

    /// <summary>
    /// Get the output directory from an instruction file.
    /// </summary>
    /// <returns>The absolute path to the output directory.</returns>
    /// <exception cref="ArgumentException">Thrown if the output directory is not specified.</exception>
    public string GetOutputDirectory()
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(outputDirectoryParameter);
        if (parameter is null)
            throw new ArgumentException($"File '{parser.FilePath}' does not specify an output directory");
        return NormalisePath(parameter.AsString());
    }

    /// <summary>
    /// Get the enabled PFTs from the instruction file.
    /// </summary>
    /// <returns>The enabled PFTs.</returns>
    public IEnumerable<string> GetEnabledPfts()
    {
        return parser.GetBlockNames(pftBlock)
            .Where(IsPftEnabled);

    }

    /// <summary>
    /// Check if a PFT is enabled.
    /// </summary>
    /// <param name="pftName">The name of the PFT.</param>
    /// <returns>True if the PFT is enabled, false otherwise.</returns>
    private bool IsPftEnabled(string pftName)
    {
        InstructionParameter? include = parser.GetBlockParameter(pftBlock, pftName, pftIncludeParameter);
        if (include is null)
        {
            logger.LogWarning("PFT '{PftName}' does not have an 'include' parameter", pftName);
            return false;
        }

        // If we don't have include 1, the PFT is disabled.
        if (include.AsInt() != 1)
        {
            logger.LogDebug("PFT {PftName} is disabled due to include {Include}", pftName, include.AsInt());
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get the number of patches in the instruction file.
    /// </summary>
    /// <returns>The number of patches.</returns>
    public int GetNumPatches()
    {
        InstructionParameter? parameter = parser.GetTopLevelParameter(npatchParameter);
        if (parameter is null)
            throw new ArgumentException($"File '{parser.FilePath}' does not specify a '{npatchParameter}' parameter");
        return parameter.AsInt();
    }

    /// <summary>
    /// Get the names of the enabled stands from an instruction file. These are
    /// all stands with stinclude 1 and run_X 1 (e.g. run_urban 1 for an urban
    /// stand).
    /// </summary>
    /// <returns>The enabled stands.</returns>
    public IEnumerable<string> GetEnabledStands()
    {
        return parser.GetBlockNames(standBlock)
                .Where(IsStandEnabled);
    }

    /// <summary>
    /// Check if a stand is enabled.
    /// </summary>
    /// <param name="standName">The name of the stand.</param>
    /// <returns>True if the stand is enabled, false otherwise.</returns>
    private bool IsStandEnabled(string standName)
    {
        InstructionParameter? include = parser.GetBlockParameter(standBlock, standName, standIncludeParameter);
        if (include is null)
        {
            logger.LogWarning("Stand '{StandName}' does not have an 'include' parameter", standName);
            return false;
        }

        // If we don't have stinclude 1, the stand is disabled.
        if (include.AsInt() != 1)
        {
            logger.LogDebug("Stand {StandName} is disabled due to stinclude {Include}", standName, include.AsInt());
            return false;
        }

        InstructionParameter? landcover = parser.GetBlockParameter(standBlock, standName, "landcover");
        if (landcover is null)
        {
            logger.LogWarning("Stand '{StandName}' does not have a 'landcover' parameter", standName);
            return false;
        }

        if (!IsLandcoverTypeEnabled(landcover.AsString()))
        {
            logger.LogDebug("Stand {StandName} is disabled because landcover type {Landcover} is disabled", standName, landcover.AsString());
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check whether the run_{landcoverType} parameter is set to 1.
    /// </summary>
    /// <param name="landcoverType">The landcover type to check.</param>
    /// <returns>True if the landcover type is enabled, false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown if the landcover type is invalid.</exception>
    private bool IsLandcoverTypeEnabled(string landcoverType)
    {
        if (!landcoverTypes.Contains(landcoverType))
            throw new ArgumentException($"Invalid landcover type '{landcoverType}'");
        string parameterName = $"run_{landcoverType}";
        InstructionParameter? parameter = parser.GetTopLevelParameter(parameterName);
        if (parameter is null)
        {
            logger.LogWarning("Instruction file {FilePath} does not contain a '{ParameterName}' parameter", parser.FilePath, parameterName);
            return false;
        }
        return parameter.AsInt() == 1;
    }

    /// <summary>
    /// Convert a relative path to an absolute path, based on the directory
    /// of the instruction file.
    /// </summary>
    /// <param name="path">The relative path.</param>
    /// <returns>The absolute path.</returns>
    private string NormalisePath(string path)
    {
        string? directory = Path.GetDirectoryName(parser.FilePath);
        if (directory == null)
            // Directory will be null if path is root directory (cannot happen),
            // or if the path doesn't contain a directory component, in which
            // case, we can assume that it is in the current directory.
            return path;

        string fullPath = Path.GetFullPath(Path.Combine(directory, path));
        return fullPath;
    }
}
