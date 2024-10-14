using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Features.Services;

namespace OrchardCore.Features.Deployment;

public class AllFeaturesDeploymentSource
    : DeploymentSourceBase<AllFeaturesDeploymentStep>
{
    private readonly IModuleService _moduleService;

    public AllFeaturesDeploymentSource(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    protected override async Task ProcessAsync(AllFeaturesDeploymentStep step, DeploymentPlanResult result)
    {
        var features = await _moduleService.GetAvailableFeaturesAsync();
        var featureStep = new JsonObject
        {
            ["name"] = "Feature",
            ["enable"] = JNode.FromObject(features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()),
        };

        if (!step.IgnoreDisabledFeatures)
        {
            featureStep.Add("disable", JNode.FromObject(features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()));
        }

        result.Steps.Add(featureStep);
    }
}
