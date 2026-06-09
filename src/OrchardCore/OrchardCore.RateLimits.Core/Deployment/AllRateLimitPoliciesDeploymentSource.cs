using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Recipes;

namespace OrchardCore.RateLimits.Deployment;

public sealed class AllRateLimitPoliciesDeploymentSource : DeploymentSourceBase<AllRateLimitPoliciesDeploymentStep>
{
    private readonly IRateLimitPolicyStore _policyStore;

    public AllRateLimitPoliciesDeploymentSource(IRateLimitPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    protected override async Task ProcessAsync(AllRateLimitPoliciesDeploymentStep step, DeploymentPlanResult result)
    {
        var document = await _policyStore.GetAsync();
        var policies = new JsonArray();

        foreach (var entry in document.Policies)
        {
            policies.Add(new JsonObject
            {
                [nameof(entry.PolicyId)] = entry.PolicyId,
                [nameof(entry.Draft)] = SerializePolicy(entry.Draft),
                [nameof(entry.Published)] = SerializePolicy(entry.Published),
                [nameof(entry.PublishedUtc)] = entry.PublishedUtc,
            });
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = CreateOrUpdateRateLimitPoliciesStep.StepKey,
            ["policies"] = policies,
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
