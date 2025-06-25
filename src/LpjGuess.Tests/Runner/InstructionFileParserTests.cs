using LpjGuess.Runner.Models;
using LpjGuess.Runner.Parsers;

namespace LpjGuess.Tests.Runner;

public class InstructionFileParserTests
{
    private const string TestFilesDir = "TestFiles";

    private string GetTestFilePath(string filename)
    {
        return Path.Combine(TestFilesDir, filename);
    }

    [Fact]
    public void Parse_C3GFile_ParsesBlockCorrectly()
    {
        string file = GetTestFilePath("c3g.ins");
        string content = File.ReadAllText(file);
        var parser = new InstructionFileParser(content, file);

        var g0Value = parser.GetBlockParameterValue("group", "C3G", "g0");
        Assert.Equal("0.161", g0Value);

        var pathwayValue = parser.GetBlockParameter("group", "C3G", "pathway");
        Assert.Equal("c3", pathwayValue!.UnquotedString);

        var harvEffValue = parser.GetBlockParameterValue("group", "C3G", "harv_eff");
        Assert.Equal("0.5", harvEffValue);
    }

    [Fact]
    public void Parse_C3GFile_HandlesExclamationInString()
    {
        string file = GetTestFilePath("c3g.ins");
        var parser = InstructionFileParser.FromFile(file);

        var messageParam = parser.GetBlockParameter("group", "test", "message");

        Assert.NotNull(messageParam);
        Assert.True(messageParam!.IsQuoted);
        Assert.Equal("Hello! World!", messageParam.UnquotedString);
    }

    [Fact]
    public void Parse_C3GFile_HandlesCommentsCorrectly()
    {
        string file = GetTestFilePath("c3g.ins");
        string content = File.ReadAllText(file);
        var parser = new InstructionFileParser(content, file);

        // These parameters have comments after them
        var tcminSurv = parser.GetBlockParameterValue("group", "C3G", "tcmin_surv");
        Assert.Equal("-1000", tcminSurv);

        var gdd5minEst = parser.GetBlockParameterValue("group", "C3G", "gdd5min_est");
        Assert.Equal("0", gdd5minEst);
    }

    [Fact]
    public void Parse_C3GFile_HandlesMultiValueParameters()
    {
        string file = GetTestFilePath("c3g.ins");
        string content = File.ReadAllText(file);
        var parser = new InstructionFileParser(content, file);

        var epsMonValues = parser.GetBlockParameterValue("group", "C3G", "eps_mon");
        Assert.Equal("0.37 0.2 0.23 0.1 0.1 0.09 0.1 0.22 0.19", epsMonValues);
    }

    [Fact]
    public void Parse_C3GFile_HandlesDifferentParameterTypes()
    {
        string file = GetTestFilePath("c3g.ins");
        string content = File.ReadAllText(file);
        var parser = new InstructionFileParser(content, file);

        // Test string parameter (quoted)
        var lifeformParam = parser.GetBlockParameter("pft", "C3G_annual", "lifeform");
        Assert.True(lifeformParam!.IsQuoted);
        Assert.Equal("grass_annual", lifeformParam.UnquotedString);

        // Test floating point parameter
        var slaParam = parser.GetBlockParameter("pft", "C3G_annual", "sla");
        Assert.True(slaParam!.TryGetDouble(out double slaValue));
        Assert.Equal(53.1, slaValue);

        // Test array parameter
        var epsMonParam = parser.GetBlockParameter("group", "C3G", "eps_mon");
        Assert.True(epsMonParam!.TryGetDoubleArray(out double[] epsMonValues));
        Assert.Equal(9, epsMonValues.Length);
        Assert.Equal(0.37, epsMonValues[0]);
        Assert.Equal(0.19, epsMonValues[^1]);

        // Test integer parameter
        var seasIsoParam = parser.GetBlockParameter("group", "C3G", "seas_iso");
        Assert.True(seasIsoParam!.TryGetInt(out int seasIsoValue));
        Assert.Equal(1, seasIsoValue);
    }

    [Fact]
    public void Parse_C3GFile_PreservesContentExactly()
    {
        string content = @"!///////////////////////////////////////////////////////////////////////////////
! C3 grass PFT settings
!///////////////////////////////////////////////////////////////////////////////

group ""C3G"" (
! Cool (C3) grass
include 1
grass
pathway ""c3""
g0 0.161
tcmin_surv -1000    ! no limit
)

pft ""C3G_annual"" (
C3G
lifeform ""grass_annual""
sla 53.1
)
";

        string file = GetTestFilePath("test.ins");
        var parser = new InstructionFileParser(content, file);

        string generatedContent = parser.GenerateContent();

        // Compare line by line to preserve exact whitespace and comments
        var originalLines = content.Split('\n');
        var generatedLines = generatedContent.Split('\n');

        Assert.Equal(originalLines.Length, generatedLines.Length);
        for (int i = 0; i < originalLines.Length; i++)
        {
            Assert.Equal(originalLines[i], generatedLines[i]);
        }
    }

    [Fact]
    public void ModifiesTopLevelParametersCorrectly()
    {
        string content = @"myparam ""test""
another_parameter 1
float_param 1.2";

        var parser = new InstructionFileParser(content, string.Empty);

        parser.SetTopLevelParameterValue("myparam", "\"new_value\"");
        parser.SetTopLevelParameterValue("another_parameter", "2");
        parser.SetTopLevelParameterValue("float_param", "2.4");

        string generatedContent = parser.GenerateContent();

        InstructionFileParser newParser = new(generatedContent, string.Empty);

        InstructionParameter myparamValue = newParser.GetTopLevelParameter("myparam")!;
        Assert.Equal("new_value", myparamValue.AsString());
        Assert.True(myparamValue.IsQuoted, "Quoting of \"new_value\" was not preserved");

        InstructionParameter anotherParameterValue = newParser.GetTopLevelParameter("another_parameter")!;
        Assert.Equal(2, anotherParameterValue.AsInt());

        InstructionParameter floatParamValue = newParser.GetTopLevelParameter("float_param")!;
        Assert.Equal(2.4, floatParamValue.AsDouble());
    }

    [Fact]
    public void Parse_C3GFile_ModifiesParametersCorrectly()
    {
        string content = @"group ""C3G"" (
g0 0.161
pathway ""c3""
)

pft ""C3G_annual"" (
lifeform ""grass_annual""
sla 53.1
)";

        string file = GetTestFilePath("test.ins");
        var parser = new InstructionFileParser(content, file);

        // Modify some parameters
        parser.SetBlockParameterValue("group", "C3G", "g0", "0.2");
        parser.SetBlockParameterValue("pft", "C3G_annual", "sla", "60.0");
        string generatedContent = parser.GenerateContent();

        var newParser = new InstructionFileParser(generatedContent, string.Empty);
        
        // Check modified values
        var g0Value = newParser.GetBlockParameter("group", "C3G", "g0");
        Assert.True(g0Value!.TryGetDouble(out double g0));
        Assert.Equal(0.2, g0);

        var slaValue = newParser.GetBlockParameter("pft", "C3G_annual", "sla");
        Assert.True(slaValue!.TryGetDouble(out double sla));
        Assert.Equal(60.0, sla);

        // Check unmodified values remain the same
        var pathwayValue = newParser.GetBlockParameter("group", "C3G", "pathway");
        Assert.Equal("c3", pathwayValue!.UnquotedString);

        var lifeformValue = newParser.GetBlockParameter("pft", "C3G_annual", "lifeform");
        Assert.Equal("grass_annual", lifeformValue!.UnquotedString);
    }

    [Fact]
    public void Parse_DuplicateParameters_HandlesCorrectly()
    {
        string content = @"group ""C3G"" (
g0 0.161     ! initial value
pathway ""c3""
g0 0.200     ! override value
)

pft ""C3G_annual"" (
lifeform ""grass_annual""
sla 45.0     ! initial value
sla 53.1     ! override value
)";

        var parser = new InstructionFileParser(content, string.Empty);

        // 1. Verify GetBlockParameter returns the last-defined value
        var g0Value = parser.GetBlockParameter("group", "C3G", "g0");
        Assert.True(g0Value!.TryGetDouble(out double g0));
        Assert.Equal(0.200, g0);

        var slaValue = parser.GetBlockParameter("pft", "C3G_annual", "sla");
        Assert.True(slaValue!.TryGetDouble(out double sla));
        Assert.Equal(53.1, sla);

        // 2. Verify that the emitted output is identical when no changes are made
        string generatedContent = parser.GenerateContent();
        Assert.Equal(content, generatedContent);

        // 3. Verify that SetBlockParameterValue modifies the last occurrence
        parser.SetBlockParameterValue("group", "C3G", "g0", "0.300");
        parser.SetBlockParameterValue("pft", "C3G_annual", "sla", "60.0");
        generatedContent = parser.GenerateContent();

        // Parse the modified content and verify values
        var newParser = new InstructionFileParser(generatedContent, string.Empty);
        
        // Check that the last values were modified
        g0Value = newParser.GetBlockParameter("group", "C3G", "g0");
        Assert.True(g0Value!.TryGetDouble(out g0));
        Assert.Equal(0.300, g0);

        slaValue = newParser.GetBlockParameter("pft", "C3G_annual", "sla");
        Assert.True(slaValue!.TryGetDouble(out sla));
        Assert.Equal(60.0, sla);

        // Verify that the structure is preserved (both occurrences still exist)
        var lines = generatedContent.Split('\n');
        Assert.Contains(lines, l => l.TrimStart().StartsWith("g0 0.161"));
        Assert.Contains(lines, l => l.TrimStart().StartsWith("g0 0.300"));
        Assert.Contains(lines, l => l.TrimStart().StartsWith("sla 45.0"));
        Assert.Contains(lines, l => l.TrimStart().StartsWith("sla 60.0"));
    }

    [Fact]
    public void Roundtrip_Reproduces_TopLevelParam_Spacing()
    {
        string content = @"not_indented_1space 0
    indented_param 1
nonindented        ""with_spaced_param""
 somewhatindented                    ""with_param_spacing_too""
param_with_comment ""xyz""! This is a comment
param_with_spaces_before_comment ""abc""        ! Comment text
";
        InstructionFileParser parser = new(content, string.Empty);

        string generatedContent = parser.GenerateContent();
        Assert.Equal(content, generatedContent);
    }

    [Fact]
    public void Parse_StartBlock_DetectsComment()
    {

        string content = @"
!st ""Urban"" (

	myparam 3.14159
	stinclude 1
	landcover ""urban""
!)";
        InstructionFileParser parser = new(content, string.Empty);
        Assert.Null(parser.GetBlockParameter("st", "Urban", "stinclude"));
        Assert.Null(parser.GetBlockParameter("!st", "Urban", "stinclude"));
    }

    [Fact]
    public void Parse_IgnoresCommentedParameters()
    {

        string content = @"
st ""Urban"" (

	common_stand
	!stinclude 1
	landcover ""urban""
)

!npatch 3";
        InstructionFileParser parser = new(content, string.Empty);
        Assert.Null(parser.GetBlockParameter("st", "Urban", "stinclude"));
        Assert.Null(parser.GetTopLevelParameter("npatch"));
    }

    [Fact]
    public void Parse_AddNewParameter_AddsAtEndOfBlock()
    {
        string content = @"group ""C3G"" (
g0 0.161     ! initial value
pathway ""c3""
)

pft ""C3G_annual"" (
lifeform ""grass_annual""
sla 53.1
)";

        var parser = new InstructionFileParser(content, string.Empty);

        // Add new parameters to both blocks
        parser.SetBlockParameterValue("group", "C3G", "newparam", "42");
        parser.SetBlockParameterValue("pft", "C3G_annual", "newparam2", "\"test value\"");
        string generatedContent = parser.GenerateContent();

        var newParser = new InstructionFileParser(generatedContent, string.Empty);
        
        // Check that the new parameters were added with correct values
        var newParamValue = newParser.GetBlockParameter("group", "C3G", "newparam");
        Assert.True(newParamValue!.TryGetDouble(out double value));
        Assert.Equal(42, value);

        var newParam2Value = newParser.GetBlockParameter("pft", "C3G_annual", "newparam2");
        Assert.Equal("test value", newParam2Value!.UnquotedString);

        // Verify that the parameters were added at the end of their blocks
        var lines = generatedContent.Split('\n');
        var g0Line = Array.FindIndex(lines, l => l.TrimStart().StartsWith("g0"));
        var newParamLine = Array.FindIndex(lines, l => l.TrimStart().StartsWith("newparam"));
        Assert.True(newParamLine > g0Line, "New parameter should be after existing parameters");

        var slaLine = Array.FindIndex(lines, l => l.TrimStart().StartsWith("sla"));
        var newParam2Line = Array.FindIndex(lines, l => l.TrimStart().StartsWith("newparam2"));
        Assert.True(newParam2Line > slaLine, "New parameter should be after existing parameters");

        // Verify indentation matches existing parameters
        var g0Indent = lines[g0Line].Length - lines[g0Line].TrimStart().Length;
        var newParamIndent = lines[newParamLine].Length - lines[newParamLine].TrimStart().Length;
        Assert.Equal(g0Indent, newParamIndent);

        var slaIndent = lines[slaLine].Length - lines[slaLine].TrimStart().Length;
        var newParam2Indent = lines[newParam2Line].Length - lines[newParam2Line].TrimStart().Length;
        Assert.Equal(slaIndent, newParam2Indent);
    }

    [Fact]
    public void TestSetTopLevelParamValue()
    {
        // fixme - this doesn't work. For now we use InstructionFile instead.

        string content = "param \"test_param\" (str \"old_value\")";
        var parser = new InstructionFileParser(content, string.Empty);

        // parser.SetBlockParameterValue("param", "test_param", "str", "new_value");
        // string newContent = parser.GenerateContent();

        // Assert.True(success);
        // Assert.Contains("param \"test_param\" (str \"new_value\")", newContent);
        // Assert.DoesNotContain("old_value", newContent);
    }

    [Fact]
    public void TestBlockStartWithNoWhitespace()
    {
        // A line like `group "lowshrub"(` is a valid block header.
        string content = @"group ""shrub""(
    g0 0.161
)
";
        var parser = new InstructionFileParser(content, string.Empty);
        double? g0 = parser.GetBlockParameter("group", "shrub", "g0")?.AsDouble();
        Assert.NotNull(g0);
        Assert.Equal(0.161, (double)g0, 1e-6);
    }
}
