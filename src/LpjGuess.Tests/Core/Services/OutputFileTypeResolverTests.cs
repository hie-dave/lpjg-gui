using LpjGuess.Core.Parsers;
using LpjGuess.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LpjGuess.Tests.Core.Services;

public class OutputFileTypeResolverTests
{
    private readonly OutputFileTypeResolver resolver;
    private readonly Mock<ILogger<OutputFileTypeResolver>> logger;
    private readonly Mock<InstructionFileParser> parser;

    public OutputFileTypeResolverTests()
    {
        logger = new Mock<ILogger<OutputFileTypeResolver>>();
        parser = new Mock<InstructionFileParser>();
        resolver = new OutputFileTypeResolver(logger.Object);
    }

    [Fact]
    public void EnsureQuotesAreStripped()
    {
        // TODO: mocking/better init
        string insFile = @"
file_lai ""lai.out""
";
        var parser = new InstructionFileParser(insFile, string.Empty);
        resolver.BuildLookupTable(parser);

        Assert.Equal("lai.out", resolver.GetFileName("file_lai"));
        Assert.Equal("file_lai", resolver.GetFileType("lai.out"));
    }

    [Fact]
    public void GetFileType_ThrowsForUnknownFilename()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => resolver.GetFileType("missing.out"));
        Assert.Contains("Unable to find output file type", ex.Message);
    }

    [Fact]
    public void GetFileName_ThrowsForUnknownFileType()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => resolver.GetFileName("file_missing"));
        Assert.Contains("Unable to find output file name", ex.Message);
    }

    [Fact]
    public void TryGetFileType_ReturnsFalseForUnknownFilename()
    {
        resolver.BuildLookupTable(new InstructionFileParser("file_lai \"lai.out\"", string.Empty));

        bool found = resolver.TryGetFileType("missing.out", out string? fileType);

        Assert.False(found);
        Assert.Null(fileType);
    }

    [Fact]
    public void GetAllFileTypes_ReturnsKnownTypesFromLookup()
    {
        resolver.BuildLookupTable(new InstructionFileParser("file_lai \"lai.out\"", string.Empty));

        HashSet<string> fileTypes = resolver.GetAllFileTypes();

        Assert.Contains("file_lai", fileTypes);
    }
}
