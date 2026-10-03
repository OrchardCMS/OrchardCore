using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Layers.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllLayersDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content", typeof(AllLayersDeploymentStep));

    public AllLayersDeploymentStep()
    {
        Name = "AllLayers";
        Category = s_category;
    }
}
