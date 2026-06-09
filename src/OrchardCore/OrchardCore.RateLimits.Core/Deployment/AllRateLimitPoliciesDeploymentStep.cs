using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.RateLimits.Recipes;

namespace OrchardCore.RateLimits.Deployment;

public sealed class AllRateLimitPoliciesDeploymentStep : DeploymentStep
{
    public AllRateLimitPoliciesDeploymentStep()
    {
        Name = CreateOrUpdateRateLimitPoliciesStep.StepKey;
    }

    public AllRateLimitPoliciesDeploymentStep(IStringLocalizer<AllRateLimitPoliciesDeploymentStep> S)
        : this()
    {
        Category = S["Security"];
    }
}
