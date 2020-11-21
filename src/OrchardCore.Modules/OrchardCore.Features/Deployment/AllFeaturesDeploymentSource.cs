using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
            var featureStep = new JObject(
                new JProperty("name", "Feature"),
                new JProperty("enable", features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray())
            );

            if (!allFeaturesStep.IgnoreDisabledFeatures)
            {
                featureStep.Property("enable").AddAfterSelf(new JProperty("disable", features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()));
            }
            result.Steps.Add(featureStep);
        }
    }
}
