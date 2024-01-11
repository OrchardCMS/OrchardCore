using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Deployment
{
    public class AllFeatureProfilesDeploymentSource : IDeploymentSource
    {
        private readonly FeatureProfilesManager _featureProfilesManager;

        public AllFeatureProfilesDeploymentSource(FeatureProfilesManager featureProfilesManager)
        {
            _featureProfilesManager = featureProfilesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllFeatureProfilesDeploymentStep)
            {
                return;
            }

            var featureProfileObjects = new JObject();
            var featureProfiles = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

            foreach (var featureProfile in featureProfiles.FeatureProfiles)
            {
                featureProfileObjects[featureProfile.Key] = JObject.FromObject(featureProfile.Value);
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "FeatureProfiles"),
                new JProperty("FeatureProfiles", featureProfileObjects)
            ));
        }
    }
}
