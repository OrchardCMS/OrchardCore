using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes
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

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                foreach (var index in indices)
                {
                    var luceneIndexSettings = index.ToObject<Dictionary<string, LuceneIndexSettings>>().FirstOrDefault();

                    if (!_luceneIndexManager.Exists(luceneIndexSettings.Key))
                    {
                        luceneIndexSettings.Value.IndexName = luceneIndexSettings.Key;
                        await _luceneIndexingService.CreateIndexAsync(luceneIndexSettings.Value);
                    }
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JObject Data { get; set; }
    }
}
