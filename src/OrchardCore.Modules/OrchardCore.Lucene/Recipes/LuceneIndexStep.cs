using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

            var model = context.Step["Indices"].ToObject<IEnumerable<ContentStepModel>>();

            //var dict = new Dictionary<string, LuceneIndexSettings>();
            //foreach (var item in model)
            //{
            //    var settings = item.Data;
            //    //dict.Add(settings.Value, settings);
            //}

            foreach (var item in dict)
            {
                if (!_luceneIndexManager.Exists(item.Key))
                {
                    await _luceneIndexingService.CreateIndexAsync(item.Value);
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JObject Data { get; set; }
    }
}
