using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Deployment;

public class AllMediaProfilesDeploymentSource : IDeploymentSource
{
    private readonly MediaProfilesManager _mediaProfilesManager;

    public AllMediaProfilesDeploymentSource(MediaProfilesManager mediaProfilesManager)
    {
        _mediaProfilesManager = mediaProfilesManager;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AllMediaProfilesDeploymentStep)
        {
            return;
        }

        var mediaProfiles = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "MediaProfiles",
            ["MediaProfiles"] = JObject.FromObject(mediaProfiles.MediaProfiles),
        });
    }
}
