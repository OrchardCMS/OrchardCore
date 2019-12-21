using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Lucene.Model;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Lucene.Recipes
{
    /// <summary>
    /// This recipe step creates a lucene index.
    /// </summary>
    public class LuceneIndexStep : IRecipeStepHandler
    {
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneIndexManager _luceneIndexManager;

        public LuceneIndexStep(
            LuceneIndexingService luceneIndexingService,
            LuceneIndexManager luceneIndexManager
            )
        {
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexingService = luceneIndexingService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "lucene-index", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indexSettingsList = context.Step["Indices"].ToObject<IEnumerable<LuceneIndexSettings>>();

            foreach(var indexSettings in indexSettingsList)
            {
                if (!_luceneIndexManager.Exists(indexSettings.IndexName))
                {
                    await _luceneIndexingService.CreateIndexAsync(indexSettings);
                }
            }
        }
    }
}
