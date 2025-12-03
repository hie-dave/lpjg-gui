using System.Threading.Tasks;
using LpjGuess.Core.Parsers;

namespace LpjGuess.Tests.Core.Parsers;

public class InstructionFileNormaliserTests
{
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
}
