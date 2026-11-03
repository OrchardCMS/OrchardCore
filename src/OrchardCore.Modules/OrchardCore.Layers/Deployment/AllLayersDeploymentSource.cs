using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment;

public sealed class AllLayersDeploymentSource
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
            ["Layers"] = JArray.FromObject(layers.Layers.Where(layer => DeploymentSelection.IsSelected(step.Names, layer.Name)).ToArray(), _jsonSerializerOptions),
        });

        // The settings are shared by all the layers, so they aren't part of a selection.
        if (step.Names is { Length: > 0 })
        {
            return;
        }

        var layerSettings = await _siteService.GetSettingsAsync<LayerSettings>();

        // Adding Layer settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "Settings",
            ["LayerSettings"] = JObject.FromObject(layerSettings),
        });
    }
}
