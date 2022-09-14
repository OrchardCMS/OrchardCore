using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes
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

            var model = context.Step.ToObject<LuceneIndexRebuildStepModel>();

            if (model.IncludeAll || model.Indices.Length > 0)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-rebuild", async (scope) =>
                {
                    var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                    var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                    var indices = model.IncludeAll ? (await luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray() : model.Indices;

                    foreach (var indexName in indices)
                    {
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

        private class LuceneIndexRebuildStepModel
        {
            public bool IncludeAll { get; set; } = false;
            public string[] Indices { get; set; } = Array.Empty<string>();
        }
    }
}
