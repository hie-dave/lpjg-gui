using LpjGuess.Core.Models;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Holds the coordinates of gridcells in a simulation, as parsed from a
/// gridlist file.
/// </summary>
/// <remarks>
/// TODO: Is this class really needed?
/// </remarks>
public class Gridlist
{
    /// <summary>
    /// The path to the gridlist file.
    /// </summary>
    private readonly string path;

    /// <summary>
    /// The parsed coordinates in the gridlist.
    /// </summary>
    private readonly IReadOnlyList<Gridcell> coordinates;

    /// <summary>
    /// The parsed coordinates in the gridlist.
    /// </summary>
    public IReadOnlyList<Gridcell> Gridcells => coordinates;

    /// <summary>
    /// Initialise a new instance of the <see cref="Gridlist"/> class.
    /// </summary>
    /// <param name="path">The path to the gridlist file.</param>
    /// <param name="parser">The parser to use to parse the gridlist file.</param>
    public Gridlist(string path, IGridlistParser parser)
    {
        this.path = path;

        // TODO: async support.
        Task<IEnumerable<Gridcell>> task = parser.ParseAsync(path);
        task.Wait();
        coordinates = task.Result.ToList();
    }

    /// <summary>
    /// Get the name of the coordinate at the specified longitude and latitude.
    /// If the coordinate is not found, an exception is thrown. If the gridcell
    /// doesn't have a name, (lat, lon) will be returned.
    /// </summary>
    /// <param name="lon">Longitude of the gridcell.</param>
    /// <param name="lat">Latitude of the gridcell.</param>
    /// <returns>The name of the gridcell.</returns>
    public string GetName(double lon, double lat)
    {
        Gridcell search = new(lat, lon, null);
        Gridcell? coordinate = coordinates.FirstOrDefault(c => c.Equals(search));
        if (coordinate is null)
            throw new ArgumentException($"Coordinate ({lat}, {lon}) not found in gridlist");
        return coordinate.Name;
    }
}
