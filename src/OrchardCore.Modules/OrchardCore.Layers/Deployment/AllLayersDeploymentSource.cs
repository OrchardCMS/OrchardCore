using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment;

public class AllLayersDeploymentSource
    : DeploymentSourceBase<AllLayersDeploymentStep>
{
    private readonly ILayerService _layerService;
    private readonly ISiteService _siteService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AllLayersDeploymentSource(
        ILayerService layerService,
        ISiteService siteService,
        IOptions<DocumentJsonSerializerOptions> serializationOptions)
    {
        _layerService = layerService;
        _siteService = siteService;
        _jsonSerializerOptions = serializationOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(AllLayersDeploymentStep step, DeploymentPlanResult result)
    {
        var layers = await _layerService.GetLayersAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Layers",
            ["Layers"] = JArray.FromObject(layers.Layers, _jsonSerializerOptions),
        });

        var layerSettings = await _siteService.GetSettingsAsync<LayerSettings>();

        // Adding Layer settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["LayerSettings"] = JObject.FromObject(layerSettings),
        });
    }
}
