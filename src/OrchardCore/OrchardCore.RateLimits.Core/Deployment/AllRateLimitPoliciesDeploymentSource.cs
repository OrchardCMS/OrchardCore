using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Recipes;

namespace OrchardCore.RateLimits.Deployment;

/// <summary>
/// Exports all rate-limit policies into a deployment plan step.
/// </summary>
public sealed class AllRateLimitPoliciesDeploymentSource : DeploymentSourceBase<AllRateLimitPoliciesDeploymentStep>
{
    private readonly IRateLimitPolicyStore _policyStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllRateLimitPoliciesDeploymentSource"/> class.
    /// </summary>
    /// <param name="policyStore">The policy store used to read current policies.</param>
    public AllRateLimitPoliciesDeploymentSource(IRateLimitPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    /// <summary>
    /// Adds all stored rate-limit policies to the deployment result.
    /// </summary>
    /// <param name="step">The deployment step being executed.</param>
    /// <param name="result">The deployment result to populate.</param>
    protected override async Task ProcessAsync(AllRateLimitPoliciesDeploymentStep step, DeploymentPlanResult result)
    {
        var policies = await _policyStore.GetAllAsync(PolicyVersion.Current);

        result.Steps.Add(new JsonObject
        {
            ["name"] = CreateOrUpdateRateLimitPoliciesStep.StepKey,
            ["policies"] = JsonSerializer.SerializeToNode(policies),
        });
    }
}
