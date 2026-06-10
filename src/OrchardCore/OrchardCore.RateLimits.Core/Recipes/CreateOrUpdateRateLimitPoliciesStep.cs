using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.RateLimits.Recipes;

/// <summary>
/// Imports draft and published rate-limit policies from a recipe step.
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
    /// Imports draft and published policies from the current recipe step.
    /// </summary>
    /// <param name="context">The recipe execution context.</param>
    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var draftPolicies = context.Step["draftPolicies"]?.AsArray();
        var publishedPolicies = context.Step["publishedPolicies"]?.AsArray();

        if (draftPolicies is null && publishedPolicies is null)
        {
            context.Errors.Add(S["At least one of 'draftPolicies' or 'publishedPolicies' is required."]);
            return;
        }

        var existingPolicies = await GetCurrentPoliciesAsync();

        await ImportPoliciesAsync(draftPolicies, existingPolicies, context, PolicyVersion.Draft);
        await ImportPoliciesAsync(publishedPolicies, existingPolicies, context, PolicyVersion.Published);
    }

    private async Task ImportPoliciesAsync(JsonArray policies, List<RateLimitPolicy> existingPolicies, RecipeExecutionContext context, PolicyVersion version)
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

            var existing = FindExistingPolicy(policy, existingPolicies);
            policy.PolicyId = existing?.PolicyId ?? policy.PolicyId ?? IdGenerator.GenerateId();

            if (existing is null)
            {
                await _policyStore.CreateAsync(policy);
            }
            else
            {
                await _policyStore.UpdateAsync(policy);
            }

            if (version == PolicyVersion.Published)
            {
                await _policyStore.PublishAsync([policy]);
            }

            Upsert(existingPolicies, await GetCurrentPolicyAsync(policy.PolicyId));
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
    {
        var draftPolicies = await _policyStore.GetAllAsync(PolicyVersion.Draft);
        var publishedPolicies = await _policyStore.GetAllAsync(PolicyVersion.Published);

        return MergePolicies(draftPolicies, publishedPolicies);
    }

    private async Task<RateLimitPolicy> GetCurrentPolicyAsync(string policyId)
    {
        var draftPolicy = await _policyStore.FindByIdAsync(policyId, PolicyVersion.Draft);
        var publishedPolicy = await _policyStore.FindByIdAsync(policyId, PolicyVersion.Published);

        return CreateCurrentPolicy(draftPolicy, publishedPolicy);
    }

    private static List<RateLimitPolicy> MergePolicies(IEnumerable<RateLimitPolicy> draftPolicies, IEnumerable<RateLimitPolicy> publishedPolicies)
    {
        var draftsById = draftPolicies.ToDictionary(x => x.PolicyId, StringComparer.Ordinal);
        var publishedById = publishedPolicies.ToDictionary(x => x.PolicyId, StringComparer.Ordinal);

        return draftsById.Keys
            .Concat(publishedById.Keys)
            .Distinct(StringComparer.Ordinal)
            .Select(policyId =>
            {
                draftsById.TryGetValue(policyId, out var draftPolicy);
                publishedById.TryGetValue(policyId, out var publishedPolicy);

                return CreateCurrentPolicy(draftPolicy, publishedPolicy);
            })
            .Where(static policy => policy is not null)
            .ToList();
    }

    private static RateLimitPolicy CreateCurrentPolicy(RateLimitPolicy draftPolicy, RateLimitPolicy publishedPolicy)
    {
        if (draftPolicy is not null)
        {
            return draftPolicy;
        }

        return publishedPolicy;
    }

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
