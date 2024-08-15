using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Deployment;

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

        var featureProfileObjects = new JsonObject();
        var featureProfiles = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

        foreach (var featureProfile in featureProfiles.FeatureProfiles)
        {
            featureProfileObjects[featureProfile.Key] = JObject.FromObject(featureProfile.Value);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "FeatureProfiles",
            ["FeatureProfiles"] = featureProfileObjects,
        });
    }
}
