using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Core.Models;

/// <summary>
/// Interface to a class which contains layer metadata for a particular output
/// file type.
/// </summary>
public interface ILayerDefinitions
{
    /// <summary>
    /// Determine if the specified layer is a data layer.
    /// </summary>
    /// <param name="layer">Name of the layer.</param>
    /// <returns>True if the layer is a data layer, false otherwise.</returns>
    bool IsDataLayer(string layer);

    /// <summary>
    /// Get the units of the specified layer.
    /// </summary>
    /// <param name="layer">Name of the layer.</param>
    /// <returns>The units of the layer.</returns>
    Unit GetUnits(string layer);
}
