using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Recipes;

public sealed class ElasticsearchIndexRecipeStep : RecipeImportStep<ElasticsearchIndexRecipeStep.ElasticsearchIndexStepModel>
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ILogger _logger;

    public ElasticsearchIndexRecipeStep(
        IIndexProfileManager indexProfileManager,
        ElasticsearchIndexManager elasticIndexManager,
        ILogger<ElasticsearchIndexRecipeStep> logger)
    {
        _indexProfileManager = indexProfileManager;
        _elasticIndexManager = elasticIndexManager;
        _logger = logger;
    }

    public override string Name => "ElasticIndexSettings";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Elasticsearch Index Settings")
            .Description("Creates or updates Elasticsearch search indexes.")
            .Required("name", "Indices")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Indices", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(ElasticsearchIndexStepModel model, RecipeExecutionContext context)
    {
        foreach (var entry in model.Indices)
        {
            foreach (var item in entry.AsObject())
            {
                var indexName = item.Key;

                if (string.IsNullOrEmpty(indexName))
                {
                    _logger.LogWarning("The Elasticsearch index name is empty. Skipping creation.");
                    continue;
                }

                var index = await _indexProfileManager.FindByNameAndProviderAsync(indexName, ElasticsearchConstants.ProviderName);

                if (index is null)
                {
                    var data = item.Value;
                    data[nameof(index.IndexName)] = indexName;

                    index = await _indexProfileManager.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource, data);

                    var validationResult = await _indexProfileManager.ValidateAsync(index);

                    if (!validationResult.Succeeded)
                    {
                        foreach (var error in validationResult.Errors)
                        {
                            context.Errors.Add(error.ErrorMessage);
                        }

                        continue;
                    }

                    await _indexProfileManager.CreateAsync(index);
                }

                var exists = await _elasticIndexManager.ExistsAsync(index.IndexFullName);

                if (!exists)
                {
                    exists = await _elasticIndexManager.CreateAsync(index);
                }

                if (exists)
                {
                    await _indexProfileManager.SynchronizeAsync(index);
                }
            }
        }
    }

    public sealed class ElasticsearchIndexStepModel
    {
        public JsonArray Indices { get; set; }
    }
}
