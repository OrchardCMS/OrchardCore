using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Lucene.Model;

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
            if (!string.Equals(context.Name, "lucene-index", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step["Indices"] is JsonArray indices)
            {
                foreach (var index in indices)
                {
                    var luceneIndexSettings = index
                        .Deserialize<Dictionary<string, LuceneIndexSettings>>()
                        .FirstOrDefault();

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
        public JsonObject Data { get; set; }
    }
}
