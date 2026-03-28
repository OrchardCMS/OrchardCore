using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.OpenSearch.Core.Services;

namespace OrchardCore.Search.OpenSearch.Core.Recipes;

/// <summary>
/// This recipe step creates an OpenSearch index.
/// </summary>
public sealed class OpenSearchIndexStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly OpenSearchIndexManager _openSearchIndexManager;
    private readonly ILogger _logger;

    public OpenSearchIndexStep(
        IIndexProfileManager indexProfileManager,
        OpenSearchIndexManager openSearchIndexManager,
        ILogger<OpenSearchIndexStep> logger
        )
        : base("OpenSearchIndexSettings")
    {
        _openSearchIndexManager = openSearchIndexManager;
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
                    _logger.LogWarning("The OpenSearch index name is empty. Skipping creation.");

                    continue;
                }

                var index = await _indexProfileManager.FindByNameAndProviderAsync(indexName, OpenSearchConstants.ProviderName);

                if (index is null)
                {
                    var data = item.Value;
                    data[nameof(index.IndexName)] = indexName;

                    index = await _indexProfileManager.NewAsync(OpenSearchConstants.ProviderName, IndexingConstants.ContentsIndexSource, data);

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

                var exists = await _openSearchIndexManager.ExistsAsync(index.IndexFullName);

                if (!exists)
                {
                    exists = await _openSearchIndexManager.CreateAsync(index);
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
