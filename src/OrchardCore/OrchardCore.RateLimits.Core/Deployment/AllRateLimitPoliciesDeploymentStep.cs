using OrchardCore.Deployment;
using OrchardCore.Localization;
using OrchardCore.RateLimits.Recipes;

namespace OrchardCore.RateLimits.Deployment;

/// <summary>
/// Represents a deployment step that exports all stored rate-limit policies.
/// </summary>
public sealed class AllRateLimitPoliciesDeploymentStep : DeploymentStep, IDeploymentStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllRateLimitPoliciesDeploymentStep"/> class.
    /// </summary>
    public override string Name => CreateOrUpdateRateLimitPoliciesStep.StepKey;

    public static LocalizationSource Category => CommonLocalizationSources.Security;
}
