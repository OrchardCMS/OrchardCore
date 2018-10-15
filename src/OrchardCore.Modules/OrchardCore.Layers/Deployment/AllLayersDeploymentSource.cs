using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly ILayerService _layerService;
        private readonly ISiteService _siteService;

        public AllLayersDeploymentSource(ILayerService layerService, ISiteService siteService)
        {
            _layerService = layerService;
            _siteService = siteService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allLayersState = step as AllLayersDeploymentStep;

            if (allLayersState == null)
            {
                return;
            }

            var layers = await _layerService.GetLayersAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "Layers"),
                new JProperty("Layers", layers.Layers.Select(JObject.FromObject))
            ));

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // Adding Layer settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LayerSettings", siteSettings.As<LayerSettings>().Zones)
            ));
        }
    }
}
