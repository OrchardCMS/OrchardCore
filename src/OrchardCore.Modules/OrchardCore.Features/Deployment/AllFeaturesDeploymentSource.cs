using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Features.Services;

namespace OrchardCore.Features.Deployment
{
    public class AllFeaturesDeploymentSource : IDeploymentSource
    {
        private readonly IModuleService _moduleService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllFeaturesDeploymentSource(IModuleService moduleService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _moduleService = moduleService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allFeaturesStep = step as AllFeaturesDeploymentStep;

            if (allFeaturesStep == null)
            {
                return;
            }

            var features = await _moduleService.GetAvailableFeaturesAsync();
            var featureStep = new JsonObject
            {
                ["name"] = "Feature",
                ["enable"] = JNode.FromObject(features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray(), _jsonSerializerOptions),
            };

            if (!allFeaturesStep.IgnoreDisabledFeatures)
            {
                featureStep.Add("disable", JNode.FromObject(features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray(), _jsonSerializerOptions));
            }

            result.Steps.Add(featureStep);
        }
    }
}
