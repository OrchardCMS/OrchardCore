using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.RateLimits.Recipes;

public sealed class CreateOrUpdateRateLimitPoliciesStep : NamedRecipeStepHandler
{
    public const string StepKey = "CreateOrUpdateRateLimitPolicies";

    private readonly IRateLimitPolicyStore _policyStore;

    internal readonly IStringLocalizer S;

    public CreateOrUpdateRateLimitPoliciesStep(
        IRateLimitPolicyStore policyStore,
        IStringLocalizer<CreateOrUpdateRateLimitPoliciesStep> stringLocalizer)
        : base(StepKey)
    {
        _policyStore = policyStore;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var policies = context.Step["policies"]?.AsArray();

        if (policies is null)
        {
            context.Errors.Add(S["The 'policies' property is required."]);
            return;
        }

        var document = await _policyStore.LoadAsync();

        foreach (var item in policies.OfType<JsonObject>())
        {
            NormalizeLimiterProperties(item);

            var entry = item.ToObject<RateLimitPolicyEntry>();
            if (entry is null)
            {
                continue;
            }

            entry.PolicyId ??= IdGenerator.GenerateId();

            if (entry.Draft is null && entry.Published is null)
            {
                context.Errors.Add(S["A policy entry must include at least a draft or published version."]);
                continue;
            }

            var existing = document.Policies.FirstOrDefault(x =>
                string.Equals(x.PolicyId, entry.PolicyId, StringComparison.Ordinal) ||
                string.Equals(x.Draft?.Name ?? x.Published?.Name, entry.Draft?.Name ?? entry.Published?.Name, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                document.Policies.Add(RateLimitPolicyStore.Clone(entry));
                continue;
            }

            existing.PolicyId = entry.PolicyId;
            existing.Draft = entry.Draft is null ? existing.Draft : RateLimitPolicyStore.Clone(entry.Draft);
            existing.Published = entry.Published is null ? existing.Published : RateLimitPolicyStore.Clone(entry.Published);
            existing.PublishedUtc = entry.PublishedUtc ?? existing.PublishedUtc;
        }

        await _policyStore.SaveAsync(document);
    }

    private static void NormalizeLimiterProperties(JsonObject item)
    {
        NormalizePolicy(item[nameof(RateLimitPolicyEntry.Draft)] as JsonObject);
        NormalizePolicy(item[nameof(RateLimitPolicyEntry.Published)] as JsonObject);
    }

    private static void NormalizePolicy(JsonObject policy)
    {
        var limiters = policy?[nameof(RateLimitPolicy.Limiters)] as JsonArray;
        if (limiters is null)
        {
            return;
        }

        foreach (var limiter in limiters.OfType<JsonObject>())
        {
            var properties = limiter[nameof(RateLimitLimiter.Properties)] as JsonObject;
            if (properties is null)
            {
                continue;
            }

            foreach (var aspect in properties.Select(static x => x.Value).OfType<JsonObject>())
            {
                foreach (var property in aspect.ToArray())
                {
                    if (property.Value is JsonValue jsonValue &&
                        jsonValue.TryGetValue<string>(out var stringValue) &&
                        int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericValue))
                    {
                        aspect[property.Key] = JsonValue.Create(numericValue);
                    }
                }
            }
        }
    }
}
