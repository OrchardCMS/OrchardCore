using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes;

/// <summary>
/// This recipe step creates a Lucene index.
/// </summary>
public sealed class LuceneIndexStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexManager;
    private readonly ILogger _logger;
    private readonly IIndexManager _luceneIndexManager;
    private readonly IIndexProfileManagementService _management;
    private readonly IStringLocalizer S;

    /// <summary>Creates the legacy recipe using shared profile creation and the Lucene provider.</summary>
    public LuceneIndexStep(
        IIndexProfileManager indexManager,
        IIndexProfileManagementService management,
        [FromKeyedServices(LuceneConstants.ProviderName)] IIndexManager luceneIndexManager,
        ILogger<LuceneIndexStep> logger,
        IStringLocalizer<LuceneIndexStep> localizer
        ) : base("lucene-index")
    {
        _luceneIndexManager = luceneIndexManager;
        _management = management;
        S = localizer;
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

                    try
                    {
                        if (await _management.CreateAsync(index) != IndexProfileManagementResult.Success)
                        {
                            context.Errors.Add(S["Unable to create the Lucene index '{0}'.", indexName]);
                        }
                    }
                    catch (IndexProfileValidationException exception)
                    {
                        foreach (var error in exception.Errors)
                        {
                            context.Errors.Add(error.ErrorMessage);
                        }
                    }

                    // Creation already schedules synchronization through the shared coordinator.
                    continue;
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
                else
                {
                    context.Errors.Add(S["Unable to create the Lucene index '{0}'.", indexName]);
                }
            }
        }
    }

    internal sealed class ContentStepModel
    {
        public JsonArray Indices { get; set; }
    }
}
