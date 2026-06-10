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
    /// <param name="policyStore">The policy store used to read current and published policies.</param>
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
        var draftPolicies = new JsonArray();
        foreach (var policy in await _policyStore.GetAllAsync(PolicyVersion.Draft))
        {
            draftPolicies.Add(SerializePolicy(policy));
        }

        var publishedPolicies = new JsonArray();
        foreach (var policy in await _policyStore.GetAllAsync(PolicyVersion.Published))
        {
            publishedPolicies.Add(SerializePolicy(policy));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = CreateOrUpdateRateLimitPoliciesStep.StepKey,
            ["draftPolicies"] = draftPolicies,
            ["publishedPolicies"] = publishedPolicies,
        });
    }

    private static JsonObject SerializePolicy(RateLimitPolicy policy)
    {
        if (policy is null)
        {
            return null;
        }

        var node = JsonSerializer.SerializeToNode(policy)?.AsObject();

        if (node is null)
        {
            return null;
        }

        node[nameof(RateLimitPolicy.OwnerId)] = "[js: parameters('AdminUserId')]";
        node[nameof(RateLimitPolicy.Author)] = "[js: parameters('AdminUsername')]";

        return node;
    }
}
