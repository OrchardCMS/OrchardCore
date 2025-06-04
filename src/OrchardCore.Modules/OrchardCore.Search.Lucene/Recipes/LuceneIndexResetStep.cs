using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step resets a Lucene index.
/// </summary>
public sealed class LuceneIndexResetStep : NamedRecipeStepHandler
{
    private readonly IIndexEntityManager _indexEntityManager;
    private readonly IServiceProvider _serviceProvider;

    public LuceneIndexResetStep(
        IIndexEntityManager indexEntityManager,
        IServiceProvider serviceProvider)
        : base("lucene-index-reset")
    {
        _indexEntityManager = indexEntityManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<LuceneIndexResetStepModel>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
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

                await _indexEntityManager.SynchronizeAsync(index);
            }

        }
    }

    private sealed class LuceneIndexResetStepModel
    {
        public bool IncludeAll { get; set; }

        public string[] Indices { get; set; } = [];
    }
}
