using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

public sealed class LuceneIndexRecipeStep : RecipeImportStep<LuceneIndexRecipeStep.LuceneIndexStepModel>
{
    private readonly IIndexProfileManager _indexManager;
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly ILogger _logger;

    public LuceneIndexRecipeStep(
        IIndexProfileManager indexManager,
        LuceneIndexManager luceneIndexManager,
        ILogger<LuceneIndexRecipeStep> logger)
    {
        _indexManager = indexManager;
        _luceneIndexManager = luceneIndexManager;
        _logger = logger;
    }

    public override string Name => "lucene-index";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Lucene Index")
            .Description("Creates or updates Lucene search indexes.")
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

    protected override async Task ImportAsync(LuceneIndexStepModel model, RecipeExecutionContext context)
    {
        foreach (var entry in model.Indices)
        {
            foreach (var item in entry.AsObject())
            {
                var indexName = item.Key;

                if (string.IsNullOrEmpty(indexName))
                {
                    _logger.LogWarning("The Lucene index name is empty. Skipping creation.");
                    continue;
                }

                var index = await _indexManager.FindByNameAndProviderAsync(indexName, LuceneConstants.ProviderName);

                if (index is null)
                {
                    var data = item.Value;
                    data[nameof(index.IndexName)] = indexName;

                    index = await _indexManager.NewAsync(LuceneConstants.ProviderName, IndexingConstants.ContentsIndexSource, data);

                    var validationResult = await _indexManager.ValidateAsync(index);

                    if (!validationResult.Succeeded)
                    {
                        foreach (var error in validationResult.Errors)
                        {
                            context.Errors.Add(error.ErrorMessage);
                        }

                        continue;
                    }

                    await _indexManager.CreateAsync(index);
                }

                var exists = await _luceneIndexManager.ExistsAsync(index.IndexFullName);

                if (!exists)
                {
                    exists = await _luceneIndexManager.CreateAsync(index);
                }

                if (exists)
                {
                    await _indexManager.SynchronizeAsync(index);
                }
            }
        }
    }

    public sealed class LuceneIndexStepModel
    {
        public JsonArray Indices { get; set; }
    }
}
