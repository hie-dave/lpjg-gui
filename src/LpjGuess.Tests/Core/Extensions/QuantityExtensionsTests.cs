using System.Data;
using LpjGuess.Core.Extensions;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Tests.Core.Extensions;

public class QuantityExtensionsTests
{
    [Fact]
    public void ToDataTable_NoLayers_ReturnsSchemaOnly()
    {
        Quantity quantity = new(
            "Q",
            "desc",
            [],
            AggregationLevel.Patch,
            TemporalResolution.Daily);

        var table = quantity.ToDataTable();

        Assert.Equal("Q", table.TableName);
        Assert.Equal(0, table.Rows.Count);
        Assert.True(table.Columns.Contains("Longitude"));
        Assert.True(table.Columns.Contains("Latitude"));
        Assert.True(table.Columns.Contains(QuantityExtensions.DateColumn));
        Assert.True(table.Columns.Contains("Stand"));
        Assert.True(table.Columns.Contains("Patch"));
    }

    [Fact]
    public void ToDataTable_DailyGridcell_MapsLayerValuesByExactTimestamp()
    {
        DateTime t = new(2001, 01, 02);
        var p = new DataPoint(t, 150.5, -33.2, 10.0);
        var p2 = p with { Value = 25.0 };

        Quantity quantity = new(
            "Q",
            "desc",
            [
                new Layer("A", new Unit("u"), [p]),
                new Layer("B", new Unit("u"), [p2])
            ],
            AggregationLevel.Gridcell,
            TemporalResolution.Daily);

        var table = quantity.ToDataTable();

        DataRow row = Assert.Single(table.Rows.Cast<DataRow>());
        Assert.Equal(150.5, (double)row["Longitude"]);
        Assert.Equal(-33.2, (double)row["Latitude"]);
        Assert.Equal(t, (DateTime)row[QuantityExtensions.DateColumn]);
        Assert.Equal(10.0, (double)row["A"]);
        Assert.Equal(25.0, (double)row["B"]);
    }

    [Fact]
    public void ToDataTable_Monthly_MatchesRowsByYearInsteadOfExactTimestamp()
    {
        var jan = new DataPoint(new DateTime(2000, 1, 1), 10, 20, 1.0);
        var jul = new DataPoint(new DateTime(2000, 7, 1), 10, 20, 9.0);

        Quantity quantity = new(
            "Q",
            "desc",
            [
                new Layer("Jan", new Unit("u"), [jan]),
                new Layer("Jul", new Unit("u"), [jul])
            ],
            AggregationLevel.Gridcell,
            TemporalResolution.Monthly);

        var table = quantity.ToDataTable();

        DataRow row = Assert.Single(table.Rows.Cast<DataRow>());
        Assert.False(table.Columns.Contains(QuantityExtensions.DateColumn));
        Assert.Equal(1.0, (double)row["Jan"]);
        Assert.Equal(9.0, (double)row["Jul"]);
    }

    [Fact]
    public void ToDataTable_Individual_AddsIdentityColumnsAndPftWhenMapped()
    {
        DateTime t = new(2005, 10, 11);
        var p = new DataPoint(t, 100, -10, 3.5, Stand: 2, Patch: 4, Individual: 7);
        var quantity = new Quantity(
            "Q",
            "desc",
            [new Layer("LAI", new Unit("m2/m2"), [p])],
            AggregationLevel.Individual,
            TemporalResolution.Daily,
            new Dictionary<int, string> { [7] = "TrBE" });

        var table = quantity.ToDataTable();

        DataRow row = Assert.Single(table.Rows.Cast<DataRow>());
        Assert.True(table.Columns.Contains("Stand"));
        Assert.True(table.Columns.Contains("Patch"));
        Assert.True(table.Columns.Contains("Individual"));
        Assert.True(table.Columns.Contains("PFT"));
        Assert.Equal(2, (int)row["Stand"]);
        Assert.Equal(4, (int)row["Patch"]);
        Assert.Equal(7, (int)row["Individual"]);
        Assert.Equal("TrBE", (string)row["PFT"]);
    }
}
