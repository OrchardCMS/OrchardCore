using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes
{
    /// <summary>
    /// This recipe step resets a lucene index.
    /// </summary>
    public class LuceneIndexResetStep : IRecipeStepHandler
    {
        private readonly ShellSettings _shellSettings;

        public LuceneIndexResetStep(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index-reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LuceneIndexResetStepModel>();

            if (model.IncludeAll || model.Indices.Length > 0)
            {
                _shellSettings.State = TenantState.Running;
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("lucene-index-reset", async (scope) => {
                    var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                    var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                    var luceneIndexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();

                    var indices = model.IncludeAll ? (await luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray() : model.Indices;

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
                                luceneIndexingService.ResetIndex(indexName);
                            }
                            await luceneIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
                _shellSettings.State = TenantState.Initializing;
            }
        }

        private class LuceneIndexResetStepModel
        {
            public bool IncludeAll { get; set; } = false;
            public string[] Indices { get; set; } = Array.Empty<string>();
        }
    }
}
