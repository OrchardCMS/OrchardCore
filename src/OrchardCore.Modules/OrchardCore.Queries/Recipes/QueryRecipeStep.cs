using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Queries.Recipes;

public sealed class QueryRecipeStep : RecipeImportStep<QueryRecipeStep.QueryStepModel>
{
    private readonly IQueryManager _queryManager;
    internal readonly IStringLocalizer S;

    public QueryRecipeStep(
        IQueryManager queryManager,
        IStringLocalizer<QueryRecipeStep> stringLocalizer)
    {
        _queryManager = queryManager;
        S = stringLocalizer;
    }

    public override string Name => "Queries";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Queries")
            .Description("Defines SQL, Lucene, or other query types.")
            .Required("name", "Queries")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Queries", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Required("Name", "Source")
                        .Properties(
                            ("Name", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Source", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Schema", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Template", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("ReturnContentItems", new RecipeStepSchemaBuilder()
                                .TypeBoolean()))
                        .AdditionalProperties(true))
                    .MinItems(1)))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(QueryStepModel model, RecipeExecutionContext context)
    {
        var queries = new List<Query>();

        foreach (var token in model.Queries.Cast<JsonObject>())
        {
            var name = token[nameof(Query.Name)]?.GetValue<string>();

            if (string.IsNullOrEmpty(name))
            {
                context.Errors.Add(S["Query name is missing or empty. The query will not be imported."]);
                continue;
            }

            var sourceName = token[nameof(Query.Source)]?.GetValue<string>();

            if (string.IsNullOrEmpty(sourceName))
            {
                context.Errors.Add(S["Could not find query source value. The query '{0}' will not be imported.", name]);
                continue;
            }

            var query = await _queryManager.GetQueryAsync(name);

            if (query == null)
            {
                query = await _queryManager.NewAsync(sourceName, token);

                if (query == null)
                {
                    context.Errors.Add(S["Could not find query source: '{0}'. The query '{1}' will not be imported.", sourceName, name]);
                    continue;
                }

                queries.Add(query);
            }
            else
            {
                await _queryManager.UpdateAsync(query, token);
            }
        }

        await _queryManager.SaveAsync(queries.ToArray());
    }

    public sealed class QueryStepModel
    {
        public JsonArray Queries { get; set; }
    }
}
