using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Deployment;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step resets an Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexResetStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public ElasticsearchIndexResetStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
        : base("elastic-index-reset")
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ElasticsearchIndexResetDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            var indexes = model.IncludeAll
            ? (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName).ConfigureAwait(false))
            : (await _indexProfileManager.GetByProviderAsync(ElasticsearchConstants.ProviderName).ConfigureAwait(false)).Where(x => model.Indices.Contains(x.IndexName));

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

                await _indexProfileManager.ResetAsync(index).ConfigureAwait(false);
                await _indexProfileManager.UpdateAsync(index).ConfigureAwait(false);

                if (!await indexManager.ExistsAsync(index.IndexFullName).ConfigureAwait(false))
                {
                    await indexManager.CreateAsync(index).ConfigureAwait(false);
                }

                await _indexProfileManager.SynchronizeAsync(index).ConfigureAwait(false);
            }
        }
    }
}
