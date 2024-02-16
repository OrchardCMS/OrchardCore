using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Deployment
{
    public class AllFeatureProfilesDeploymentSource : IDeploymentSource
    {
        private readonly FeatureProfilesManager _featureProfilesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllFeatureProfilesDeploymentSource(
            FeatureProfilesManager featureProfilesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _featureProfilesManager = featureProfilesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllFeatureProfilesDeploymentStep)
            {
                return;
            }

            var featureProfileObjects = new JsonObject();
            var featureProfiles = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

            foreach (var featureProfile in featureProfiles.FeatureProfiles)
            {
                featureProfileObjects[featureProfile.Key] = JObject.FromObject(featureProfile.Value, _jsonSerializerOptions);
            }

            result.Steps.Add(new JsonObject
            {
                ["name"] = "FeatureProfiles",
                ["FeatureProfiles"] = featureProfileObjects,
            });
        }
    }
}
