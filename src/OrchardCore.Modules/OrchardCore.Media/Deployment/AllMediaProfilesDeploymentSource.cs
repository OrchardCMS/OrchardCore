using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Deployment;

public class AllMediaProfilesDeploymentSource
    : DeploymentSourceBase<AllMediaProfilesDeploymentStep>
{
    private readonly MediaProfilesManager _mediaProfilesManager;

    public AllMediaProfilesDeploymentSource(MediaProfilesManager mediaProfilesManager)
    {
        _mediaProfilesManager = mediaProfilesManager;
    }

    protected override async Task ProcessAsync(AllMediaProfilesDeploymentStep step, DeploymentPlanResult result)
    {
        var mediaProfiles = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "MediaProfiles",
            ["MediaProfiles"] = JObject.FromObject(mediaProfiles.MediaProfiles),
        });
    }
}
