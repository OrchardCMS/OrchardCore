using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdValidationDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "OpenID Validation";

    public static LocalizationSource Category => OpenIdLocalizationSources.OpenIdConnect;
}
