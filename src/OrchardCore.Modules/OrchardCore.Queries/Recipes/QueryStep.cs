using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Queries.Recipes;

/// <summary>
/// This recipe step creates a set of queries.
/// </summary>
public sealed class QueryStep : NamedRecipeStepHandler
{
    private readonly IQueryManager _queryManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    internal readonly IStringLocalizer S;

    public QueryStep(
        IQueryManager queryManager,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IStringLocalizer<QueryStep> stringLocalizer)
        : base("Queries")
    {
        _queryManager = queryManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<QueryStepModel>(_jsonSerializerOptions);

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
}

public sealed class QueryStepModel
{
    public JsonArray Queries { get; set; }
}
