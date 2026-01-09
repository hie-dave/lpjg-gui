using System.Diagnostics;
using System.Security.Cryptography;
using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Runner.Services;

namespace LpjGuess.Tests.Runner.Services;

public class NamingStrategyTests
{
    private sealed class TestSimulation : ISimulation
    {
        public string Name { get; }
        public IEnumerable<IFactor> Changes { get; }
        public TestSimulation(string name)
        {
            Name = name;
            Changes = Array.Empty<IFactor>();
        }
        public void Generate(string insFile, string targetFile, IEnumerable<string> pfts)
        {
            // no-op for naming tests
        }
    }

    [Fact]
    public void ManualNamingStrategy_Returns_Simulation_Name()
    {
        var strategy = new ManualNamingStrategy();
        var sim = new TestSimulation("This Is My Name");
        string result = strategy.GenerateName(sim);
        Assert.Equal("This Is My Name", result);
    }

    [Fact]
    public void HashNamingStrategy_Is_Deterministic_For_Same_Name()
    {
        var strategy = new Sha256NamingStrategy();
        var sim1 = new TestSimulation("alpha_beta_gamma");
        var sim2 = new TestSimulation("alpha_beta_gamma");

        string a = strategy.GenerateName(sim1);
        string b = strategy.GenerateName(sim2);

        Assert.Equal(a, b);
    }

    [Fact]
    public void HashNamingStrategy_Differs_For_Different_Names()
    {
        var strategy = new Sha256NamingStrategy();
        var sim1 = new TestSimulation("alpha");
        var sim2 = new TestSimulation("beta");

        string a = strategy.GenerateName(sim1);
        string b = strategy.GenerateName(sim2);

        Assert.NotEqual(a, b);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(32)]
    public void HashNamingStrategy_HasCorrectLength(int length)
    {
        Debug.Assert(length <= SHA256.HashSizeInBytes * 2);
        var strategy = new Sha256NamingStrategy { MaxLength = length };
        var sim = new TestSimulation("alpha");
        string name = strategy.GenerateName(sim);
        Assert.Equal(length, name.Length);
    }
}
