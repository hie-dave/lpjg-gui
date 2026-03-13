using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Tests.Core.Models.Entities;

public class EntityAndRequestModelTests
{
    [Fact]
    public void DatasetEntityDefaults_AreInitialized()
    {
        var datasetGroup = new DatasetGroup();
        var observation = new ObservationDataset();
        var prediction = new PredictionDataset();
        var variable = new Variable();
        var layer = new VariableLayer();
        var individual = new Individual();
        var pft = new Pft();

        Assert.Empty(datasetGroup.Datasets);
        Assert.Equal("{}", datasetGroup.Metadata);

        Assert.Empty(observation.Variables);
        Assert.Equal("{}", observation.Metadata);
        Assert.Equal(string.Empty, observation.Source);

        Assert.Equal("{}", prediction.Metadata);
        Assert.Equal(Array.Empty<byte>(), prediction.Patches);
        Assert.Equal(string.Empty, prediction.BaselineChannel);

        Assert.Empty(variable.Layers);
        Assert.Equal(string.Empty, variable.Name);

        Assert.Equal(string.Empty, layer.Name);
        Assert.Equal(string.Empty, layer.Description);

        Assert.Empty(individual.Data);
        Assert.Equal(string.Empty, pft.Name);
        Assert.Empty(pft.Individuals);
    }

    [Fact]
    public void PredictionDataset_SetAndGetPatches_RoundTrips()
    {
        var prediction = new PredictionDataset();
        const string patches = "diff --git a/file b/file\n+line";

        prediction.SetPatches(patches);
        string roundTripped = prediction.GetPatches();

        Assert.NotEmpty(prediction.Patches);
        Assert.Equal(patches, roundTripped);
    }

    [Fact]
    public void DatumInheritance_AllowsSettingHierarchySpecificProperties()
    {
        var datum = new IndividualDatum
        {
            Id = 1,
            Value = 2.5,
            Timestamp = new DateTime(2000, 1, 1),
            Longitude = 120.0,
            Latitude = -30.0,
            VariableId = 3,
            LayerId = 4,
            StandId = 5,
            PatchId = 6,
            IndividualId = 7
        };

        Assert.Equal(5, datum.StandId);
        Assert.Equal(6, datum.PatchId);
        Assert.Equal(7, datum.IndividualId);
        Assert.Equal(2.5, datum.Value);
        Assert.Equal(120.0, datum.Longitude);
        Assert.Equal(-30.0, datum.Latitude);
    }

    [Fact]
    public void ImporterRequestDefaults_AreInitialized()
    {
        var groupRequest = new CreateDatasetGroupRequest();
        var datasetRequest = new CreateDatasetRequest();
        var variableRequest = new CreateVariableRequest();
        var layerRequest = new CreateLayerRequest();
        var appendRequest = new AppendDataRequest();

        Assert.Equal(string.Empty, groupRequest.Name);
        Assert.Equal("{}", groupRequest.Metadata);

        Assert.Equal(string.Empty, datasetRequest.Name);
        Assert.Equal("{}", datasetRequest.Metadata);
        Assert.Equal(Array.Empty<byte>(), datasetRequest.CompressedCodePatches);

        Assert.Null(variableRequest.IndividualPfts);
        Assert.Null(variableRequest.Name);
        Assert.Null(layerRequest.Name);
        Assert.Null(appendRequest.DataPoints);
    }
}
