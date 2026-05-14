using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Layers.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllLayersDeploymentStep : DeploymentStep
{
    public AllLayersDeploymentStep()
    {
        Name = "AllLayers";
    }

    public AllLayersDeploymentStep(IStringLocalizer<AllLayersDeploymentStep> S)
        : this()
    {
        Category = S["Content"];
    }
}
