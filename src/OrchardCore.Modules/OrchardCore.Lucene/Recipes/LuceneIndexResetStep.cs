using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index-reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-reset", async (scope) => {
                    var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                    var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                    var luceneIndexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();
                    foreach (var indexToken in indices)
                    {
                        var indexName = indexToken.ToObject<string>();
                        var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);
                        if (luceneIndexSettings != null)
                        {
                            if (!luceneIndexManager.Exists(indexName))
                            {
                                await luceneIndexingService.CreateIndexAsync(luceneIndexSettings);
                            }
                            else
                            {
                                luceneIndexingService.ResetIndex(indexName);
                            }
                            await luceneIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
            }
        }
    }
}
