using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes
{
    /// <summary>
    /// This recipe step rebuilds a lucene index.
    /// </summary>
    public class LuceneIndexRebuildStep : IRecipeStepHandler
    {
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index-rebuild", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-rebuild", async (scope) => {
                    var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                    var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                    foreach (var indexToken in indices)
                    {
                        var indexName = indexToken.ToObject<string>();
                        var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);
                        if (luceneIndexSettings != null)
                        {
                            await luceneIndexingService.RebuildIndexAsync(indexName);
                            await luceneIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
            }
        }
    }
}
