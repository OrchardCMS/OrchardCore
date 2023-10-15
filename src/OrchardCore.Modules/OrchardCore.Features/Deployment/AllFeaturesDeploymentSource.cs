using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Features.Services;

namespace OrchardCore.Features.Deployment
{
    public class AllFeaturesDeploymentSource : IDeploymentSource
    {
        private readonly IModuleService _moduleService;

        public AllFeaturesDeploymentSource(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allFeaturesStep = step as AllFeaturesDeploymentStep;

            if (allFeaturesStep == null)
            {
                return;
            }

            var features = await _moduleService.GetAvailableFeaturesAsync();
            var enable = features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray();
            var disable = allFeaturesStep.IgnoreDisabledFeatures
                ? Array.Empty<string>()
                : features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray();

            var featureStep = new JsonObject { ["name"] = "Feature" };

            if (disable.Any())
            {
                featureStep["disable"] = JsonSerializer.SerializeToNode(disable);
            }

            if (enable.Any())
            {
                featureStep["enable"] = JsonSerializer.SerializeToNode(enable);
            }

            result.Steps.Add(featureStep);
        }
    }
}
