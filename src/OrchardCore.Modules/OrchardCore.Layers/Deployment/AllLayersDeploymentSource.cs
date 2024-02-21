using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly ILayerService _layerService;
        private readonly ISiteService _siteService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllLayersDeploymentSource(
            ILayerService layerService,
            ISiteService siteService,
            IOptions<JsonSerializerOptions> serializationOptions)
        {
            _layerService = layerService;
            _siteService = siteService;
            _jsonSerializerOptions = serializationOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllLayersDeploymentStep)
            {
                return;
            }

            var layers = await _layerService.GetLayersAsync();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Layers",
                ["Layers"] = JArray.FromObject(layers.Layers, _jsonSerializerOptions),
            });

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // Adding Layer settings
            result.Steps.Add(new JsonObject
            {
                ["name"] = "Settings",
                ["LayerSettings"] = JObject.FromObject(siteSettings.As<LayerSettings>()),
            });
        }
    }
}
