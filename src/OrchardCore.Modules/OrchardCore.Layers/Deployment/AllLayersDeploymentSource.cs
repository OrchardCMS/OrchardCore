using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Layers.Services;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly ILayerService _layerService;

        public AllLayersDeploymentSource(ILayerService layerService)
        {
            _layerService = layerService;
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
        }
    }
}
