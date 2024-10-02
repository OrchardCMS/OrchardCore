using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting.Recipes;

/// <summary>
/// This recipe step creates a set of url rewrites.
/// </summary>
public sealed class UrlRewritingStep : IRecipeStepHandler
{
    private readonly RewriteRulesStore _rewriteRulesStore;

    internal readonly IStringLocalizer S;

    public UrlRewritingStep(
        RewriteRulesStore rewriteRulesStore,
        IStringLocalizer<UrlRewritingStep> stringLocalizer)
    {
        _rewriteRulesStore = rewriteRulesStore;
        S = stringLocalizer;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "UrlRewriting", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<UrlRewritingStepModel>();

        foreach (var importedRule in model.Rules)
        {
            if (string.IsNullOrWhiteSpace(importedRule.Name))
            {
                context.Errors.Add(S["Unable to add or update url rewriting rule. The rule name cannot be null or empty."]);
                continue;
            }

            if (string.IsNullOrEmpty(importedRule.Substitution))
            {
                context.Errors.Add(S["Unable to add or update url rewriting rule '{0}'. The rule substitution cannot be null or empty.", importedRule.Name]);
                continue;
            }

            await _rewriteRulesStore.SaveAsync(importedRule);
        }
    }
}

public sealed class UrlRewritingStepModel
{
    public RewriteRule[] Rules { get; set; }
}
