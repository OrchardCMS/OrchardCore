using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Facebook.Services;

namespace OrchardCore.Facebook.Deployment;

public class FacebookLoginDeploymentSource
    : DeploymentSourceBase<FacebookLoginDeploymentStep>
{
    private readonly IFacebookService _facebookService;

    public FacebookLoginDeploymentSource(IFacebookService facebookService)
    {
        _facebookService = facebookService;
    }

    protected override async Task ProcessAsync(FacebookLoginDeploymentStep step, DeploymentPlanResult result)
    {
        var settings = await _facebookService.GetSettingsAsync();

        // The 'name' property should match the related recipe step name.
        var jObject = new JsonObject { ["name"] = "FacebookCoreSettings" };

        // Merge settings as the recipe step doesn't use a child property.
        jObject.Merge(JObject.FromObject(settings));

        result.Steps.Add(jObject);
    }
}
