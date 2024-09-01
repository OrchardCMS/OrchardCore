using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Features.Services;

namespace OrchardCore.Features.Deployment;

public class AllFeaturesDeploymentSource : IDeploymentSource
{
    private readonly IModuleService _moduleService;

    public AllFeaturesDeploymentSource(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AllFeaturesDeploymentStep allFeaturesStep)
        {
            return;
        }

        var features = await _moduleService.GetAvailableFeaturesAsync();
        var featureStep = new JsonObject
        {
            ["name"] = "Feature",
            ["enable"] = JNode.FromObject(features.Where(f => f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()),
        };

        if (!allFeaturesStep.IgnoreDisabledFeatures)
        {
            featureStep.Add("disable", JNode.FromObject(features.Where(f => !f.IsEnabled).Select(f => f.Descriptor.Id).ToArray()));
        }

        result.Steps.Add(featureStep);
    }
}
