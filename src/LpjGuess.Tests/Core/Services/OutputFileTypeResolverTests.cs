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
}
