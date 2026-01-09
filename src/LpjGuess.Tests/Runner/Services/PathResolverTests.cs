using LpjGuess.Runner.Services;

namespace LpjGuess.Tests.Runner.Services;

public class PathResolverTests
{
    [Theory]
    [InlineData("/a/b", "index.txt", "index.txt")]
    [InlineData("/x/y", "/x/y/z/test.txt", "z/test.txt")]
    public void TestGetRelativePath(string outputDirectory, string path, string expected)
    {
        IPathResolver resolver = CreatePathResolver(outputDirectory);
        string result = resolver.GetRelativePath(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("/p/q/r", "xyz", "/p/q/r/xyz")]
    [InlineData("/a/s/d/f", "/x/y/z", "/x/y/z")]
    public void TestGetAbsolutePath(string outputDirectory, string relative, string expected)
    {
        IPathResolver resolver = CreatePathResolver(outputDirectory);
        string result = resolver.GetAbsolutePath(relative);
        Assert.Equal(expected, result);
    }

    private IPathResolver CreatePathResolver(string outputDirectory)
    {
        return new StaticPathResolver(outputDirectory, new ManualNamingStrategy());
    }
}
