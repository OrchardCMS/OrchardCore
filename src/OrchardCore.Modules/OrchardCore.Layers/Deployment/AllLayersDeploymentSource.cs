using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly ILayerService _layerService;
        private readonly ISiteService _siteService;
        private readonly JsonSerializerOptions _serializationOptions;

        public AllLayersDeploymentSource(
            ILayerService layerService,
            ISiteService siteService,
            IOptions<JsonDerivedTypesOptions> derivedTypesOptions)
        {
            _layerService = layerService;
            _siteService = siteService;

            // The recipe step contains polymorphic types which need to be resolved
            _serializationOptions = new()
            {
                TypeInfoResolver = new PolymorphicJsonTypeInfoResolver(derivedTypesOptions.Value)
            };
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allLayersStep = step as AllLayersDeploymentStep;

            if (allLayersStep == null)
            {
                return;
            }

            var layers = await _layerService.GetLayersAsync();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Layers",
                ["Layers"] = JArray.FromObject(layers.Layers, _serializationOptions),
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
