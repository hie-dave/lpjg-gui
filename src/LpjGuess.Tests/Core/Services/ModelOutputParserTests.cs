using Microsoft.Extensions.Logging;
using Moq;
using LpjGuess.Core.Services;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Tests.Core.Services;

public class ModelOutputParserTests : IAsyncLifetime
{
    /// <summary>
    /// Tolerance for floating point comparisons.
    /// </summary>
    const double eps = 0.001;

    private readonly string _testDir;
    private readonly ModelOutputParser _parser;
    private readonly Mock<ILogger<ModelOutputParser>> _logger;
    private readonly Mock<IOutputFileTypeResolver> _resolver;

    public ModelOutputParserTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "output_tests");
        _logger = new Mock<ILogger<ModelOutputParser>>();
        _resolver = new Mock<IOutputFileTypeResolver>();
        _parser = new ModelOutputParser(_logger.Object, _resolver.Object);

        // Setup resolver to return known file types
        _resolver.Setup(r => r.GetFileType(It.IsAny<string>()))
            .Returns<string>(filename => 
            {
                string name = Path.GetFileNameWithoutExtension(filename);
                return name switch
                {
                    "lai" => "file_lai",
                    "indiv_lai" => "file_dave_indiv_lai",
                    _ => throw new InvalidOperationException($"Unknown file type: {name}")
                };
            });
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_testDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Directory.Delete(_testDir, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ParseOutputFile_ValidFormat_Success()
    {
        string content = @"Lon Lat Year TeBE TeNE
151.25 -33.25 2000 2.5 1.8
151.25 -33.75 2000 2.6 1.9
151.50 -33.75 2000 1.8 1.7";
        string filePath = Path.Combine(_testDir, "lai.out");
        await File.WriteAllTextAsync(filePath, content);

        Quantity quantity = await _parser.ParseOutputFileAsync(filePath);

        Assert.NotNull(quantity); // Tautology?
        Assert.Equal(2, quantity.Layers.Count); // TeBE and TeNE

        Layer tebe = quantity.Layers.First(l => l.Name == "TeBE");
        Layer tene = quantity.Layers.First(l => l.Name == "TeNE");

        // Verify units.
        Assert.Equal("m2/m2", tebe.Unit.Name); // Units inferred from file name.
        Assert.Equal("m2/m2", tene.Unit.Name); // Units inferred from file name.

        // Verify coordinates.
        Assert.Equal(151.25, tebe.Data[0].Longitude, eps);
        Assert.Equal(-33.25, tebe.Data[0].Latitude, eps);

        Assert.Equal(151.25, tebe.Data[1].Longitude, eps);
        Assert.Equal(-33.75, tebe.Data[1].Latitude, eps);

        Assert.Equal(151.50, tebe.Data[2].Longitude, eps);
        Assert.Equal(-33.75, tebe.Data[2].Latitude, eps);

        Assert.Equal(151.25, tene.Data[0].Longitude, eps);
        Assert.Equal(-33.25, tene.Data[0].Latitude, eps);

        Assert.Equal(151.25, tene.Data[1].Longitude, eps);
        Assert.Equal(-33.75, tene.Data[1].Latitude, eps);

        Assert.Equal(151.50, tene.Data[2].Longitude, eps);
        Assert.Equal(-33.75, tene.Data[2].Latitude, eps);

        // Verify timestamps.
        // The model writes day of year starting at 0, so day 1 is January 2.
        Assert.Equal(new DateTime(2000, 12, 30), tebe.Data[0].Timestamp);
        Assert.Equal(new DateTime(2000, 12, 30), tebe.Data[1].Timestamp);
        Assert.Equal(new DateTime(2000, 12, 30), tebe.Data[2].Timestamp);

        Assert.Equal(new DateTime(2000, 12, 30), tene.Data[0].Timestamp);
        Assert.Equal(new DateTime(2000, 12, 30), tene.Data[1].Timestamp);
        Assert.Equal(new DateTime(2000, 12, 30), tene.Data[2].Timestamp);

        // Verify data.
        Assert.Equal(3, tebe.Data.Count); // 3 rows
        Assert.Equal(2.5, tebe.Data[0].Value, eps);
        Assert.Equal(2.6, tebe.Data[1].Value, eps);
        Assert.Equal(1.8, tebe.Data[2].Value, eps);

        Assert.Equal(3, tene.Data.Count); // 3 rows
        Assert.Equal(1.8, tene.Data[0].Value, eps);
        Assert.Equal(1.9, tene.Data[1].Value, eps);
        Assert.Equal(1.7, tene.Data[2].Value, eps);
    }

    [Fact]
    public async Task ParseIndivOutputFile_Succeeds()
    {
        string content = @"Lon      Lat    Year     Day          stand          patch          indiv              pft            lai
131.15   -12.50    2003       0              0              0             20             TrBE     3.76705464
131.15   -12.50    2003       1              0              0             20             TrBE     3.76188102
131.15   -12.50    2003       2              0              0             20             TrBE     3.75671514
131.15   -12.50    2003       3              0              0             20             TrBE     3.75155710
131.15   -12.50    2003       4              0              0             20             TrBE     3.74640696
131.15   -12.50    2003       5              0              0             20             TrBE     3.74126464";

        string filePath = Path.Combine(_testDir, "indiv_lai.out");
        await File.WriteAllTextAsync(filePath, content);

        Quantity quantity = await _parser.ParseOutputFileAsync(filePath);

        // Check PFT mappings.
        Assert.NotNull(quantity.IndividualPfts);
        (int indiv, string pft) = Assert.Single(quantity.IndividualPfts);
        Assert.Equal("TrBE", pft);
        Assert.Equal(20, indiv);

        // Check Data.
        Layer layer = Assert.Single(quantity.Layers);
        Assert.Equal(6, layer.Data.Count);
    }

    [Fact]
    public async Task ParseOutputFile_InvalidHeader_ThrowsException()
    {
        // Arrange
        var content = @"Invalid Header Format
151.25 -33.75 2000 1 2.5 10.2";
        var filePath = Path.Combine(_testDir, "lai.out");
        await File.WriteAllTextAsync(filePath, content);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidDataException>(
            () => _parser.ParseOutputFileAsync(filePath));
        
        Assert.Contains("Invalid number of columns", ex.Message);

        // Verify error was logged
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid number of columns")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ParseOutputFile_InvalidFileType_ThrowsException()
    {
        // Arrange
        var content = @"Lon Lat Year Day LAI NPP
151.25 -33.75 2000 1 invalid 10.2";
        var filePath = Path.Combine(_testDir, "laif");
        await File.WriteAllTextAsync(filePath, content);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _parser.ParseOutputFileAsync(filePath));
        
        Assert.Contains("Unknown file type", ex.Message);
    }

    [Fact]
    public async Task ParseOutputFile_InvalidDataRow_ThrowsException()
    {
        // Arrange
        var content = @"Lon Lat Year Day LAI NPP
151.25 -33.75 2000 1 invalid 10.2";
        var filePath = Path.Combine(_testDir, "lai.out");
        await File.WriteAllTextAsync(filePath, content);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidDataException>(
            () => _parser.ParseOutputFileAsync(filePath));
        
        Assert.Contains("failed to parse double", ex.Message);

        // Verify error was logged
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed to parse double")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ParseOutputFile_EmptyFile_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "lai.out");
        await File.WriteAllTextAsync(filePath, string.Empty);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidDataException>(
            () => _parser.ParseOutputFileAsync(filePath));
        
        Assert.Contains("at least a header row", ex.Message);

        // Verify error was logged
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("at least a header row")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ParseOutputFile_UnknownFileKind_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "empty.out");
        await File.WriteAllTextAsync(filePath, string.Empty);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _parser.ParseOutputFileAsync(filePath));
        
        Assert.Contains("Unknown file type", ex.Message);
    }
}
