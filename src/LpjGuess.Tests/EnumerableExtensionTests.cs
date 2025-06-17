using LpjGuess.Core.Extensions;

namespace LpjGuess.Tests;

public class EnumerableExtensionTests
{
    [Fact]
    public void TestAllCombinations()
    {
        List<List<int>> input = new List<List<int>>
        {
            new List<int> { 0, 1 },
            new List<int> { 2, 3 },
            new List<int> { 4, 5 }
        };

        List<List<int>> expectedOutput = new List<List<int>>
        {
            new List<int> { 0, 2, 4 },
            new List<int> { 0, 2, 5 },
            new List<int> { 0, 3, 4 },
            new List<int> { 0, 3, 5 },
            new List<int> { 1, 2, 4 },
            new List<int> { 1, 2, 5 },
            new List<int> { 1, 3, 4 },
            new List<int> { 1, 3, 5 }
        };

        List<List<int>> result = input.AllCombinations().Select(l => l.ToList()).ToList();

        Assert.Equal(expectedOutput, result);
    }
}
