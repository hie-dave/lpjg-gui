using LpjGuess.Core.Interfaces.Factorial;
using LpjGuess.Core.Models.Dtos;
using LpjGuess.Core.Models.Factorial.Factors;

namespace LpjGuess.Tests.Core.Models.Dtos;

public class FactorDtoTests
{
    [Fact]
    public void TopLevelParameter_RoundTrip_PreservesFields()
    {
        var factor = new TopLevelParameter("alpha", "1.2");

        FactorDto dto = FactorDto.FromFactor(factor);
        IFactor roundTrip = dto.ToFactor();

        Assert.IsType<TopLevelParameter>(roundTrip);
        var tl = (TopLevelParameter)roundTrip;
        Assert.Equal("alpha", tl.Name);
        Assert.Equal("1.2", tl.Value);
        Assert.Null(dto.BlockName);
        Assert.Null(dto.BlockType);
    }

    [Fact]
    public void BlockParameter_RoundTrip_PreservesFields()
    {
        var factor = BlockParameter.Pft("pine", "sla", "3.4");

        FactorDto dto = FactorDto.FromFactor(factor);
        IFactor roundTrip = dto.ToFactor();

        Assert.IsType<BlockParameter>(roundTrip);
        var bp = (BlockParameter)roundTrip;
        Assert.Equal("pft", bp.BlockType);
        Assert.Equal("pine", bp.BlockName);
        Assert.Equal("sla", bp.Name);
        Assert.Equal("3.4", bp.Value);
        Assert.Equal("pine", dto.BlockName);
        Assert.Equal("pft", dto.BlockType);
    }

    [Fact]
    public void EmptyDto_ToFactor_ReturnsDummyFactor()
    {
        var dto = new FactorDto();

        IFactor f = dto.ToFactor();

        Assert.IsType<DummyFactor>(f);
    }
}
