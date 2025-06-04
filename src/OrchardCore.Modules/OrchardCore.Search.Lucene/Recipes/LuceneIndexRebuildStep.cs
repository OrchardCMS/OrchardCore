using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step rebuilds a Lucene index.
/// </summary>
public sealed class LuceneIndexRebuildStep : NamedRecipeStepHandler
{
    private readonly IIndexEntityManager _indexEntityManager;
    private readonly IServiceProvider _serviceProvider;

    public LuceneIndexRebuildStep(
        IIndexEntityManager indexEntityManager,
        IServiceProvider serviceProvider)
        : base("lucene-index-rebuild")
    {
        _indexEntityManager = indexEntityManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<LuceneIndexRebuildStepModel>();

        if (model.IncludeAll || model.Indices.Length > 0)
        {
            var indexes = model.IncludeAll
            ? (await _indexEntityManager.GetByProviderAsync(LuceneConstants.ProviderName))
            : (await _indexEntityManager.GetByProviderAsync(LuceneConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

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

    private sealed class LuceneIndexRebuildStepModel
    {
        public bool IncludeAll { get; set; }

        public string[] Indices { get; set; } = [];
    }
}
