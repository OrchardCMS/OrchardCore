using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Recipes;

/// <summary>
/// This recipe step creates or updates a set of URL rewrite rule.
/// </summary>
public sealed class UrlRewritingStep : NamedRecipeStepHandler
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IRewriteRulesManager _rewriteRulesManager;

    internal readonly IStringLocalizer S;

    public UrlRewritingStep(
        IRewriteRulesManager rewriteRulesManager,
        IOptions<JsonSerializerOptions> jsonSerializerOptions,
        IStringLocalizer<UrlRewritingStep> stringLocalizer)
        : base("UrlRewriting")
    {
        _rewriteRulesManager = rewriteRulesManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<UrlRewritingStepModel>(_jsonSerializerOptions);
        var tokens = model.Rules.Cast<JsonObject>() ?? [];

        foreach (var token in tokens)
        {
            RewriteRule rule = null;

            var id = token[nameof(RewriteRule.Id)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(id))
            {
                rule = await _rewriteRulesManager.FindByIdAsync(id);

                if (rule != null)
                {
                    await _rewriteRulesManager.UpdateAsync(rule, token);
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

                rule = await _rewriteRulesManager.NewAsync(sourceName, token);

                if (rule == null)
                {
                    context.Errors.Add(S["Unable to find a rule-source that can handle the source '{Source}'.", sourceName]);

                    continue;
                }
            }

            var validationResult = await _rewriteRulesManager.ValidateAsync(rule);

            if (!validationResult.Succeeded)
            {
                foreach (var error in validationResult.Errors)
                {
                    context.Errors.Add(error.ErrorMessage);
                }

                continue;
            }

            await _rewriteRulesManager.SaveAsync(rule);
        }
    }
}

public sealed class UrlRewritingStepModel
{
    public JsonArray Rules { get; set; }
}
