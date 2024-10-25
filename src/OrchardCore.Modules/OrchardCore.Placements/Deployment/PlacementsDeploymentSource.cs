using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Placements.Services;

namespace OrchardCore.Placements.Deployment;

public class PlacementsDeploymentSource
    : DeploymentSourceBase<PlacementsDeploymentStep>
{
    private readonly PlacementsManager _placementsManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public PlacementsDeploymentSource(
        PlacementsManager placementsManager,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _placementsManager = placementsManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(PlacementsDeploymentStep step, DeploymentPlanResult result)
    {
        var placementObjects = new JsonObject();
        var placements = await _placementsManager.ListShapePlacementsAsync();

        foreach (var placement in placements)
        {
            placementObjects[placement.Key] = JArray.FromObject(placement.Value, _jsonSerializerOptions);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Placements",
            ["Placements"] = placementObjects,
        });
    }
}
