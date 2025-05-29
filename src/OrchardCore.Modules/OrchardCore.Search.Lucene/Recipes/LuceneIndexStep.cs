using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step creates a Lucene index.
/// </summary>
public sealed class LuceneIndexStep : NamedRecipeStepHandler
{
    private readonly IIndexEntityManager _indexManager;
    private readonly ILogger _logger;
    private readonly LuceneIndexManager _luceneIndexManager;

    public LuceneIndexStep(
        IIndexEntityManager indexManager,
        LuceneIndexManager luceneIndexManager,
        ILogger<LuceneIndexStep> logger
        ) : base("lucene-index")
    {
        _luceneIndexManager = luceneIndexManager;
        _indexManager = indexManager;
        _logger = logger;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var settings = context.Step.ToObject<ContentStepModel>();

        foreach (var entry in settings.Indices)
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
}

public sealed class ContentStepModel
{
    public JsonArray Indices { get; set; }
}
