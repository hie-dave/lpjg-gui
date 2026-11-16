using System.Threading.Tasks;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Tests.Core.Parsers;

public class InstructionFileNormaliserTests : IDisposable
{
    private readonly TempDirectory tempDirectory;

    public InstructionFileNormaliserTests()
    {
        tempDirectory = TempDirectory.Create();
    }

    public void Dispose()
    {
        tempDirectory.Dispose();
    }

    [Fact]
    public async Task Constructor_SetsPathProperty()
    {
        const string filename = "test_instructions.ins";
        const string content = "param \"file_met_forcing\" (str \"meteo.nc\")";

        string testFilePath = Path.Combine(tempDirectory.AbsolutePath, filename);
        await File.WriteAllTextAsync(testFilePath, content);

        var instructionFile = new InstructionFileNormaliser(testFilePath);

        Assert.Equal(testFilePath, instructionFile.FilePath);
    }

    [Fact]
    public void Constructor_ThrowsWhenFileNotFound()
    {
        var nonExistentPath = Path.Combine(tempDirectory.AbsolutePath, "nonexistent.ins");
        Assert.Throws<FileNotFoundException>(() => new InstructionFileNormaliser(nonExistentPath));
    }
    /// <summary>
    /// Repro for a bug in which normalising an instruction file with a relative
    /// path fails.
    /// </summary>
    [Fact]
    public async Task NormaliseRelativeInsFile()
    {
        using TempDirectory temp = TempDirectory.Create();
        string childPath = Path.Combine(temp.AbsolutePath, "child");
        using TempDirectory child = new TempDirectory(childPath);

        const string fileName = "file.ins";
        string insPath = Path.Combine(temp.AbsolutePath, fileName);
        await File.WriteAllTextAsync(insPath, @"
file_gridlist ""../other.txt""
");

        // This may fail if temporary directory cannot be made relative to the
        // current directory. E.g. if it's on a different drive on windows(?).
        string cwd = Directory.GetCurrentDirectory();
        string relativePath = Path.GetRelativePath(cwd, insPath);
        InstructionFileNormaliser.Normalise(relativePath);
    }

    [Theory]
    [InlineData("soildata", "soildata.txt")]
    [InlineData("custom_input", "soil data !(sample$1.txt")]
    public async Task Normaliser_NormalisesFileBlocks(string blockName, string basename)
    {
        using TempDirectory temp = TempDirectory.Create();
        string childPath = Path.Combine(temp.AbsolutePath, "child");
        using TempDirectory child = new TempDirectory(childPath);

        string file = Path.Combine(child.AbsolutePath, "file.ins");
        await File.WriteAllTextAsync(file, $"file \"{blockName}\" (path \"../{basename}\")\n");

        string expected = Path.Combine(temp.AbsolutePath, basename);
        string normalised = InstructionFileNormaliser.Normalise(file);
        Assert.Contains(expected, normalised);
    }

    [Theory]
    [InlineData("soildata", "soildata.txt")]
    [InlineData("custom_input", "soil data !(sample$1.txt")]
    public async Task Normaliser_NormalisesMultilineFileBlocks(string blockName, string basename)
    {
        using TempDirectory temp = TempDirectory.Create();
        string childPath = Path.Combine(temp.AbsolutePath, "child");
        using TempDirectory child = new TempDirectory(childPath);

        string file = Path.Combine(child.AbsolutePath, "file.ins");
        await File.WriteAllTextAsync(file, $@"
file ""{blockName}"" (
    path ""../{basename}""
)");

        string expected = Path.Combine(temp.AbsolutePath, basename);
        string normalised = InstructionFileNormaliser.Normalise(file);
        Assert.Contains(expected, normalised);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public async Task Normaliser_PreservesFileBlockFormattingAndNestedColumns(string lineEnding)
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "file.ins");
        string content = """
            ! file "disabled" (path "unchanged.dat")
            file "soildata" ( ! Soil input (with a comment)
                path "../forcing/soil data !(sample$1.dat" ! Input path

                ! Use soil data from closest gridcell, within this distance.
                max_distance 50

                ! Define custom column names.
                column "orgc" (source "orgC")
                column "cn" (
                    source "CN"
                )
                column "ph" (source "pH")
            )
            npatch 1
            """.Replace("\n", lineEnding);
        await File.WriteAllTextAsync(file, content);

        string absolute = Path.GetFullPath("../forcing/soil data !(sample$1.dat", temp.AbsolutePath);
        string expected = content.Replace("\"../forcing/soil data !(sample$1.dat\"", $"\"{absolute}\"");
        string normalised = InstructionFileNormaliser.Normalise(file);

        Assert.Equal(expected, normalised);
        await File.WriteAllTextAsync(file, normalised);
        Assert.Equal(normalised, InstructionFileNormaliser.Normalise(file));
    }

    [Theory]
    [InlineData("param \"file_soildata\" (str \"../soil.dat\")")]
    [InlineData("param \"file_soildata\" (\n    str \"../soil.dat\" ! Soil path\n)")]
    [InlineData("param \"file_soildata\" ( ! Soil input\n\n    ! Path to data\n    str \"../soil.dat\"\n)")]
    [InlineData("file_soildata \"../soil.dat\" ! Soil path")]
    public async Task Normaliser_NormalisesLegacyPaths(string content)
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "file.ins");
        await File.WriteAllTextAsync(file, content);

        string absolute = Path.GetFullPath("../soil.dat", temp.AbsolutePath);
        string expected = content.Replace("\"../soil.dat\"", $"\"{absolute}\"");

        Assert.Equal(expected, InstructionFileNormaliser.Normalise(file));
    }

    [Theory]
    [InlineData("")]
    [InlineData("! Only comments\n")]
    [InlineData("file \"soildata\" (path \"\")\nparam \"file_ndep\" (str \"\")\nfile_co2 \"\"")]
    [InlineData("file \"soildata\" (max_distance 50)")]
    public async Task Normaliser_LeavesContentWithoutPathsUnchanged(string content)
    {
        using TempDirectory temp = TempDirectory.Create();
        string file = Path.Combine(temp.AbsolutePath, "file.ins");
        await File.WriteAllTextAsync(file, content);

        Assert.Equal(content, InstructionFileNormaliser.Normalise(file));
    }

    [Fact]
    public async Task Normaliser_NormalisesFileBlocksInImports()
    {
        using TempDirectory temp = TempDirectory.Create();
        string root = Path.Combine(temp.AbsolutePath, "root.ins");
        string child = Path.Combine(temp.AbsolutePath, "child.ins");
        string grandchild = Path.Combine(temp.AbsolutePath, "grandchild.ins");
        await File.WriteAllTextAsync(root, "import \"child.ins\"\nnpatch 1\n");
        await File.WriteAllTextAsync(child, "import \"grandchild.ins\"\n");
        const string content = "file \"soildata\" (\n    path \"../soil.dat\"\n)\n";
        await File.WriteAllTextAsync(grandchild, content);

        string absolute = Path.GetFullPath("../soil.dat", temp.AbsolutePath);
        string expected = content.Replace("\"../soil.dat\"", $"\"{absolute}\"") + "npatch 1\n";

        Assert.Equal(expected, InstructionFileNormaliser.Normalise(root));
        Assert.Equal(new[] { child, grandchild }, InstructionFileNormaliser.ResolveImportDirectives(root));
        Assert.NotNull(InstructionFileParser.FromFile(root).GetBlockParameter("file", "soildata", "path"));
    }
}
