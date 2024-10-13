using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Recipes;

/// <summary>
/// This recipe step creates a set of url rewrites.
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

        var rules = new List<RewriteRule>();

        foreach (var token in model.Rules.Cast<JsonObject>())
        {
            var name = token[nameof(RewriteRule.Name)]?.GetValue<string>();

            if (string.IsNullOrEmpty(name))
            {
                context.Errors.Add(S["Rule name is missing or empty. The rule will not be imported."]);

                continue;
            }

            var sourceName = token[nameof(RewriteRule.Source)]?.GetValue<string>();

            if (string.IsNullOrEmpty(sourceName))
            {
                context.Errors.Add(S["Could not find rule source value. The rule '{0}' will not be imported.", name]);

                continue;
            }

            var rule = await _rewriteRulesManager.NewAsync(sourceName, token);

            rules.Add(rule);
        }
    }
}

public sealed class UrlRewritingStepModel
{
    public JsonArray Rules { get; set; }
}
