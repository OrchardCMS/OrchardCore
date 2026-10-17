using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.OpenId.Deployment;

/// <summary>
/// Adds Open ID settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class OpenIdValidationDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<OpenIdValidationDeploymentStep>("OpenID Connect");

    public OpenIdValidationDeploymentStep()
    {
        Name = "OpenID Validation";
        Category = s_category;
    }
}
