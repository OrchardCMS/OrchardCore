using System;
using System.Threading.Tasks;
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
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneIndexingService _luceneIndexingService;

        public LuceneIndexRebuildStep(
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneIndexingService luceneIndexingService
            )
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneIndexingService = luceneIndexingService;
        }

        private async Task RebuildIndexAsync(string indexName)
        {
            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(indexName);

            if (luceneIndexSettings != null)
            {
                await _luceneIndexingService.RebuildIndexAsync(indexName);
                await _luceneIndexingService.ProcessContentItemsAsync(indexName);
            }
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index-rebuild", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-rebuild", async (shellScope) => {
                    foreach (var indexName in indices)
                    {
                        await RebuildIndexAsync(indexName.ToObject<string>());
                    }
                });
            }
        }
    }
}
