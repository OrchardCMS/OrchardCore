using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly static JsonSerializer _jsonSerializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private readonly ILayerService _layerService;
        private readonly ISiteService _siteService;

        public AllLayersDeploymentSource(ILayerService layerService, ISiteService siteService)
        {
            _layerService = layerService;
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allLayersStep = step as AllLayersDeploymentStep;

            if (allLayersStep == null)
            {
                return;
            }

            var layers = await _layerService.GetLayersAsync();
            var layersJson = layers.Layers.Select(layer => JsonSerializer.SerializeToNode(layer, _jsonSerializer)).ToArray();
            result.AddSimpleStep("Layers", "Layers", new JsonArray(layersJson));

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // Adding Layer settings
            result.AddSimpleStepAndSerializeValue("Settings", "LayerSettings", siteSettings.As<LayerSettings>());
        }
    }
}
