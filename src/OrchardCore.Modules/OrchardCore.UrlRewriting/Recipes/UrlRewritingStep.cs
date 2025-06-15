using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Recipes;

/// <summary>
/// This recipe step creates or updates a set of URL rewrite rule.
/// </summary>
public sealed class UrlRewritingStep : NamedRecipeStepHandler
{
    private readonly IRewriteRulesManager _rewriteRulesManager;

    internal readonly IStringLocalizer S;

    public UrlRewritingStep(
        IRewriteRulesManager rewriteRulesManager,
        IStringLocalizer<UrlRewritingStep> stringLocalizer)
        : base("UrlRewriting")
    {
        _rewriteRulesManager = rewriteRulesManager;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<UrlRewritingStepModel>();
        var tokens = model.Rules.Cast<JsonObject>() ?? [];

        foreach (var token in tokens)
        {
            RewriteRule rule = null;

            var id = token[nameof(RewriteRule.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                rule = await _rewriteRulesManager.FindByIdAsync(id).ConfigureAwait(false);

                if (rule != null)
                {
                    await _rewriteRulesManager.UpdateAsync(rule, token).ConfigureAwait(false);
                }
            }

            if (rule == null)
            {
                var sourceName = token[nameof(RewriteRule.Source)]?.GetValue<string>();

                if (string.IsNullOrEmpty(sourceName))
                {
                    context.Errors.Add(S["Could not find rule source value. The rule will not be imported"]);

                    continue;
                }

                rule = await _rewriteRulesManager.NewAsync(sourceName, token).ConfigureAwait(false);

                if (rule == null)
                {
                    context.Errors.Add(S["Unable to find a rule-source that can handle the source '{Source}'.", sourceName]);

                    continue;
                }
            }

            var validationResult = await _rewriteRulesManager.ValidateAsync(rule).ConfigureAwait(false);

            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    context.Errors.Add(error.ErrorMessage);
                }

                continue;
            }

            await _rewriteRulesManager.SaveAsync(rule).ConfigureAwait(false);
        }
    }

    private sealed class UrlRewritingStepModel
    {
        public JsonArray Rules { get; set; }
    }
}
