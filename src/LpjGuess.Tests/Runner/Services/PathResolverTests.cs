using LpjGuess.Runner.Services;

namespace LpjGuess.Tests.Runner.Services;

public class PathResolverTests
{
    [Fact]
    public void TestGetRelativePath_NonRootedPath()
    {
        using TempDirectory temp = TempDirectory.Create();
        const string path = "index.txt";
        IPathResolver resolver = CreatePathResolver(temp.AbsolutePath);
        string result = resolver.GetRelativePath(path);
        Assert.Equal(path, result);
    }

    [Fact]
    public void TestGetRelativePath_RootedPath()
    {
        using TempDirectory temp = TempDirectory.Create();
        string path = Path.Combine(temp.AbsolutePath, "subdir", "file.dat");
        IPathResolver resolver = CreatePathResolver(temp.AbsolutePath);
        string result = resolver.GetRelativePath(path);
        Assert.Equal(Path.Combine("subdir", "file.dat"), result);
    }

    [Fact]
    public void TestGetAbsolutePath_NonRootedPath()
    {
        using TempDirectory temp = TempDirectory.Create();
        const string relative = "xyz";
        IPathResolver resolver = CreatePathResolver(temp.AbsolutePath);
        string result = resolver.GetAbsolutePath(relative);
        Assert.Equal(Path.Combine(temp.AbsolutePath, relative), result);
    }

    [Fact]
    public void TestGetAbsolutePath_RootedPath()
    {
        const string relative = "abc.def";

        // /tmp/asdf
        using TempDirectory temp = TempDirectory.Create();

        // /tmp/asdf/abc.def
        string rooted = Path.Combine(temp.AbsolutePath, relative);

        // Attempt to resolve the fully rooted path to an absolute path.
        IPathResolver resolver = CreatePathResolver(temp.AbsolutePath);
        string result = resolver.GetAbsolutePath(rooted);

        // Should return the original path.
        Assert.Equal(rooted, result);
    }

    private IPathResolver CreatePathResolver(string outputDirectory)
    {
        return new StaticPathResolver(outputDirectory, new ManualNamingStrategy());
    }
}
