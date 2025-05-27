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
    private readonly IIndexEntityManager _indexEntityManager;
    private readonly IServiceProvider _serviceProvider;

    public ElasticsearchIndexRebuildStep(
        IIndexEntityManager indexEntityManager,
        IServiceProvider serviceProvider)
        : base("elastic-index-rebuild")
    {
        _indexEntityManager = indexEntityManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ElasticsearchIndexRebuildDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            var indexes = model.IncludeAll
            ? (await _indexEntityManager.GetAsync(ElasticsearchConstants.ProviderName))
            : (await _indexEntityManager.GetAsync(ElasticsearchConstants.ProviderName)).Where(x => model.Indices.Contains(x.Id));

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

                await _indexEntityManager.ResetAsync(index);
                await _indexEntityManager.UpdateAsync(index);

                if (!await indexManager.ExistsAsync(index.IndexFullName))
                {
                    await indexManager.CreateAsync(index);
                }
                else
                {
                    await indexManager.RebuildAsync(index);
                }

                await _indexEntityManager.SynchronizeAsync(index);
            }
        }
    }
}
