using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Layers.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllLayersDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllLayers";

    public static LocalizationSource Category => CommonLocalizationSources.Content;
}
