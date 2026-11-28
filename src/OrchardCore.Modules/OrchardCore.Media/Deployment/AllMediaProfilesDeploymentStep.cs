using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds media profiles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllMediaProfilesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllMediaProfiles";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;
}
