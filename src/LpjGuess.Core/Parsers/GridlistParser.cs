using LpjGuess.Core.Utility;

namespace LpjGuess.Core.Parsers;

/// <summary>
/// Parser for LPJ-GUESS gridlist files.
/// </summary>
public class GridlistParser
{
    /// <summary>
    /// The path to the gridlist file.
    /// </summary>
    private readonly string path;

    /// <summary>
    /// The parsed coordinates in the gridlist.
    /// </summary>
    private readonly IReadOnlyList<Coordinate> coordinates;

    /// <summary>
    /// Initialise a new instance of the <see cref="GridlistParser"/> class.
    /// </summary>
    /// <param name="path">The path to the gridlist file.</param>
    public GridlistParser(string path)
    {
        this.path = path;

        // TODO: async support.
        Task<IReadOnlyList<Coordinate>> task = ParseAsync();
        task.Wait();
        coordinates = task.Result;
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
        Coordinate search = new(lat, lon, null);
        Coordinate? coordinate = coordinates.FirstOrDefault(c => c.Equals(search));
        if (coordinate is null)
            throw new ArgumentException($"Coordinate ({lat}, {lon}) not found in gridlist");
        return coordinate.Name ?? $"({lat}, {lon})";
    }

    /// <summary>
    /// Parse the gridlist file.
    /// </summary>
    /// <returns>The parsed gridlist.</returns>
    private async Task<IReadOnlyList<Coordinate>> ParseAsync(CancellationToken ct = default)
    {
        List<Coordinate> gridlist = new();

        await foreach (string line in File.ReadLinesAsync(path, ct))
            if (!string.IsNullOrWhiteSpace(line))
                gridlist.Add(ParseLine(line));

        return gridlist;
    }

    /// <summary>
    /// Parse a coordinate from a line.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>The parsed coordinate.</returns>
    /// <exception cref="FormatException">Thrown if the line is invalid.</exception>
    private static Coordinate ParseLine(string line)
    {
        StringSplitOptions opts = StringSplitOptions.RemoveEmptyEntries |
                                  StringSplitOptions.TrimEntries;
        string[] parts = line.Split(' ', opts);
        if (parts.Length < 2)
            throw new FormatException($"Invalid gridlist line: {line}");
        double longitude = double.Parse(parts[0]);
        double latitude = double.Parse(parts[1]);
        string? name = parts.Length > 2 ? parts[2] : null;
        return new Coordinate(latitude, longitude, name);
    }

    private class Coordinate
    {
        private readonly double eps;
        public double Latitude { get; private init; }
        public double Longitude { get; private init; }
        public string? Name { get; private init; }

        public Coordinate(double latitude, double longitude, string? name, double eps = 1e-2)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
            this.eps = eps;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Coordinate other)
                return MathUtility.AreEqual(Latitude, other.Latitude, eps) &&
                       MathUtility.AreEqual(Longitude, other.Longitude, eps);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude);
        }

        public override string ToString()
        {
            return $"({Latitude}, {Longitude})";
        }
    }
}
