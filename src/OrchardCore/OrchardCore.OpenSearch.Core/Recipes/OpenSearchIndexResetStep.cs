using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.OpenSearch.Core.Deployment;

namespace OrchardCore.OpenSearch.Core.Recipes;

/// <summary>
/// This recipe step resets an OpenSearch index.
/// </summary>
public sealed class OpenSearchIndexResetStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public OpenSearchIndexResetStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
        : base("opensearch-index-reset")
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenSearchIndexResetDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            var indexes = model.IncludeAll
            ? (await _indexProfileManager.GetByProviderAsync(OpenSearchConstants.ProviderName))
            : (await _indexProfileManager.GetByProviderAsync(OpenSearchConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

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

                await _indexProfileManager.ResetAsync(index);
                await _indexProfileManager.UpdateAsync(index);

                if (!await indexManager.ExistsAsync(index.IndexFullName))
                {
                    await indexManager.CreateAsync(index);
                }

                await _indexProfileManager.SynchronizeAsync(index);
            }
        }
    }
}
