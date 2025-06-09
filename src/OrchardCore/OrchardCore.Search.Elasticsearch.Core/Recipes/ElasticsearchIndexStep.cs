using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step creates a Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ILogger _logger;

    public ElasticsearchIndexStep(
        IIndexProfileManager indexProfileManager,
        ElasticsearchIndexManager elasticIndexManager,
        ILogger<ElasticsearchIndexStep> logger
        )
        : base("ElasticIndexSettings")
    {
        _elasticIndexManager = elasticIndexManager;
        _logger = logger;
        _indexProfileManager = indexProfileManager;
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

    internal sealed class ContentStepModel
    {
        public JsonArray Indices { get; set; }
    }
}
