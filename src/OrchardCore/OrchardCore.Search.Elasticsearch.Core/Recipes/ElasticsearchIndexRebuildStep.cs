using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step rebuilds an Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexRebuildStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public ElasticsearchIndexRebuildStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
        : base("elastic-index-rebuild")
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ElasticsearchIndexRebuildDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            var indexes = model.IncludeAll
            ? (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName))
            : (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

            var indexManagers = new Dictionary<string, IIndexManager>();

            foreach (var index in indexes)
            {
                if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                {
                    indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                    indexManagers[index.ProviderName] = indexManager;
                }

                if (indexManager is null)
                {
                    continue;
                }

                var reset = await _indexProfileManager.ResetAsync(index);

                if (reset)
                {
                    await _indexProfileManager.UpdateAsync(index);
                }
                bool rebuilt;
                if (!await indexManager.ExistsAsync(index.IndexFullName))
                {
                    await indexManager.CreateAsync(index);

                    rebuilt = true;
                }
                else
                {
                    // Always attempt to rebuild the index even if we failed to reset it.
                    rebuilt = await indexManager.RebuildAsync(index);
                }

                if (rebuilt && reset)
                {
                    await _indexProfileManager.SynchronizeAsync(index);
                }
            }
        }
    }
}
