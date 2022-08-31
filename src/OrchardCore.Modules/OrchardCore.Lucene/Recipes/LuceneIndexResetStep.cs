using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes
{
    /// <summary>
    /// This recipe step resets a lucene index.
    /// </summary>
    public class LuceneIndexResetStep : IRecipeStepHandler
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneIndexManager _luceneIndexManager;

        public LuceneIndexResetStep(
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneIndexingService luceneIndexingService,
            LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexingService = luceneIndexingService;
        }

        private async Task ResetIndexAsync(string indexName)
        {
            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(indexName);

            if (luceneIndexSettings != null)
            {
                if (!_luceneIndexManager.Exists(indexName))
                {
                    await _luceneIndexingService.CreateIndexAsync(luceneIndexSettings);
                    await _luceneIndexingService.ProcessContentItemsAsync(indexName);
                }
                else
                {
                    _luceneIndexingService.ResetIndex(indexName);
                    await _luceneIndexingService.ProcessContentItemsAsync(indexName);
                }
            }
            await Task.CompletedTask;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index-reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-reset", async (shellScope) => {
                    foreach (var indexName in indices)
                    {
                        await ResetIndexAsync(indexName.ToObject<string>());
                    }
                });
            }
        }
    }
}
