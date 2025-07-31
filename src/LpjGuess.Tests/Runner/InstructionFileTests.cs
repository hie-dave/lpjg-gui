using System;
using System.IO;
using LpjGuess.Core.Parsers;
using LpjGuess.Runner;
using Xunit;

namespace LpjGuess.Tests.Runner;

public class InstructionFileTests : IDisposable
{
    private readonly string _testFilePath;

    public InstructionFileTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), "test_instructions.ins");
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }

    [Fact]
    public void Constructor_SetsPathProperty()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "param \"file_met_forcing\" (str \"meteo.nc\")");

        // Act
        var instructionFile = new InstructionFileNormaliser(_testFilePath);

        // Assert
        Assert.Equal(_testFilePath, instructionFile.FilePath);
    }

    [Fact]
    public void Constructor_ThrowsWhenFileNotFound()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent.ins");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new InstructionFileNormaliser(nonExistentPath));
    }

    [Fact]
    public void GetParamValue_ReturnsCorrectValue()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "param \"test_param\" (str \"test_value\")");
        var instructionFile = new InstructionFileNormaliser(_testFilePath);

        // Act
        var value = instructionFile.GetParamValue("test_param", File.ReadAllText(_testFilePath));

        // Assert
        Assert.Equal("test_value", value);
    }

    [Fact]
    public void TryGetParamValue_ReturnsNullForMissingParam()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "param \"test_param\" (str \"test_value\")");
        var instructionFile = new InstructionFileNormaliser(_testFilePath);

        // Act
        var value = instructionFile.TryGetParamValue("missing_param", File.ReadAllText(_testFilePath));

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void SetParamValue_UpdatesParameterCorrectly()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "param \"test_param\" (str \"old_value\")");
        var instructionFile = new InstructionFileNormaliser(_testFilePath);
        var contents = File.ReadAllText(_testFilePath);

        // Act
        var newContents = instructionFile.SetParamValue("test_param", "new_value", contents);

        // Assert
        Assert.Contains("param \"test_param\" (str \"new_value\")", newContents);
        Assert.DoesNotContain("old_value", newContents);
    }
}
