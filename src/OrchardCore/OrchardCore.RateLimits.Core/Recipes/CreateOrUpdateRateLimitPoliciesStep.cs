using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.RateLimits.Recipes;

/// <summary>
/// Imports rate-limit policies from a recipe step.
/// </summary>
public sealed class CreateOrUpdateRateLimitPoliciesStep : NamedRecipeStepHandler
{
    /// <summary>
    /// The recipe step name used to import rate-limit policies.
    /// </summary>
    public const string StepKey = "CreateOrUpdateRateLimitPolicies";

    private readonly IRateLimitPolicyStore _policyStore;

    internal readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrUpdateRateLimitPoliciesStep"/> class.
    /// </summary>
    /// <param name="policyStore">The policy store used to create and update policies.</param>
    /// <param name="stringLocalizer">The string localizer used for recipe validation errors.</param>
    public CreateOrUpdateRateLimitPoliciesStep(
        IRateLimitPolicyStore policyStore,
        IStringLocalizer<CreateOrUpdateRateLimitPoliciesStep> stringLocalizer)
        : base(StepKey)
    {
        _policyStore = policyStore;
        S = stringLocalizer;
    }

    /// <summary>
    /// Imports policies from the current recipe step.
    /// </summary>
    /// <param name="context">The recipe execution context.</param>
    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var policies = context.Step["policies"]?.AsArray();

        if (policies is null)
        {
            context.Errors.Add(S["The 'policies' array is required."]);
            return;
        }

        var existingPolicies = await GetCurrentPoliciesAsync();

        await ImportPoliciesAsync(policies, existingPolicies, context);
    }

    private async Task ImportPoliciesAsync(JsonArray policies, List<RateLimitPolicy> existingPolicies, RecipeExecutionContext context)
    {
        if (policies is null)
        {
            return;
        }

        foreach (var item in policies.OfType<JsonObject>())
        {
            var policy = item.ToObject<RateLimitPolicy>();
            if (policy is null || !ValidatePolicy(policy, context))
            {
                continue;
            }

            if (!policy.IsEnabled)
            {
                policy.EnabledUtc = null;
            }

            var existing = FindExistingPolicy(policy, existingPolicies);

            policy.PolicyId = existing?.PolicyId ?? policy.PolicyId ?? IdGenerator.GenerateId();

            policy.EnabledUtc = policy.IsEnabled
                ? policy.EnabledUtc ?? existing?.EnabledUtc ?? DateTime.UtcNow
                : null;

            if (existing is null)
            {
                await _policyStore.CreateAsync(policy);
            }
            else
            {
                await _policyStore.UpdateAsync(policy);
            }

            Upsert(existingPolicies, await _policyStore.FindByIdAsync(policy.PolicyId, PolicyVersion.Current));
        }
    }

    private bool ValidatePolicy(RateLimitPolicy policy, RecipeExecutionContext context)
    {
        if (policy.Scope != RateLimitPolicyScope.Global && policy.Scope != RateLimitPolicyScope.Endpoint)
        {
            context.Errors.Add(S["The policy '{0}' has an unsupported scope.", policy.Name ?? policy.PolicyId ?? string.Empty]);
            return false;
        }

        if (policy.Scope == RateLimitPolicyScope.Endpoint)
        {
            if (string.IsNullOrWhiteSpace(policy.Path))
            {
                context.Errors.Add(S["The endpoint policy '{0}' must define a request path.", policy.Name ?? policy.PolicyId ?? string.Empty]);
                return false;
            }

            if (!policy.Path.StartsWith('/'))
            {
                context.Errors.Add(S["The endpoint policy '{0}' must use a request path that starts with '/'.", policy.Name ?? policy.PolicyId ?? string.Empty]);
                return false;
            }
        }

        return true;
    }

    private async Task<List<RateLimitPolicy>> GetCurrentPoliciesAsync()
        => [.. await _policyStore.GetAllAsync(PolicyVersion.Current)];

    private static RateLimitPolicy FindExistingPolicy(RateLimitPolicy policy, IEnumerable<RateLimitPolicy> existingPolicies)
    {
        return existingPolicies.FirstOrDefault(x =>
            (!string.IsNullOrEmpty(policy.PolicyId) && string.Equals(x.PolicyId, policy.PolicyId, StringComparison.Ordinal)) ||
            string.Equals(x.Name, policy.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static void Upsert(List<RateLimitPolicy> policies, RateLimitPolicy policy)
    {
        if (policy is null)
        {
            return;
        }

        var index = policies.FindIndex(x => string.Equals(x.PolicyId, policy.PolicyId, StringComparison.Ordinal));
        if (index == -1)
        {
            policies.Add(policy);
            return;
        }

        policies[index] = policy;
    }
}
