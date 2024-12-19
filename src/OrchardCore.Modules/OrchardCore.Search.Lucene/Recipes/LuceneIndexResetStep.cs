using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step resets a Lucene index.
/// </summary>
public sealed class LuceneIndexResetStep : NamedRecipeStepHandler
{
    public LuceneIndexResetStep()
        : base("lucene-index-reset")
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<LuceneIndexResetStepModel>();

        if (model.IncludeAll || model.Indices.Length > 0)
        {
            await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-reset", async (scope) =>
            {
                var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                var luceneIndexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();

                var indices = model.IncludeAll
                ? (await luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray()
                : model.Indices;

                foreach (var indexName in indices)
                {
                    var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);
                    if (luceneIndexSettings != null)
                    {
                        if (!luceneIndexManager.Exists(indexName))
                        {
                            await luceneIndexingService.CreateIndexAsync(luceneIndexSettings);
                        }
                        else
                        {
                            luceneIndexingService.ResetIndexAsync(indexName);
                        }
                        await luceneIndexingService.ProcessContentItemsAsync(indexName);
                    }
                }
            });
        }
    }

    private sealed class LuceneIndexResetStepModel
    {
        public bool IncludeAll { get; set; }
        public string[] Indices { get; set; } = [];
    }
}
