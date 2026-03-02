using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Recipes;

public sealed class UrlRewritingRecipeStep : RecipeImportStep<UrlRewritingRecipeStep.UrlRewritingStepModel>
{
    private readonly IRewriteRulesManager _rewriteRulesManager;

    internal readonly IStringLocalizer S;

    public UrlRewritingRecipeStep(
        IRewriteRulesManager rewriteRulesManager,
        IStringLocalizer<UrlRewritingRecipeStep> stringLocalizer)
    {
        _rewriteRulesManager = rewriteRulesManager;
        S = stringLocalizer;
    }

    public override string Name => "UrlRewriting";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("URL Rewriting")
            .Description("Creates or updates URL rewrite rules.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Rules", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(UrlRewritingStepModel model, RecipeExecutionContext context)
    {
        var tokens = model.Rules?.Cast<JsonObject>() ?? [];

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

    public sealed class UrlRewritingStepModel
    {
        public JsonArray Rules { get; set; }
    }
}
