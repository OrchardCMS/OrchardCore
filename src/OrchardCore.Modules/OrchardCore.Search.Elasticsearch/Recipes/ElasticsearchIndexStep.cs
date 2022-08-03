using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Model;

namespace OrchardCore.Search.Elasticsearch.Recipes
{
    /// <summary>
    /// This recipe step creates a Elasticsearch index.
    /// </summary>
    public class ElasticsearchIndexStep : IRecipeStepHandler
    {
        private readonly ElasticsearchIndexingService _elasticIndexingService;
        private readonly ElasticsearchIndexManager _elasticIndexManager;

        public ElasticsearchIndexStep(
            ElasticsearchIndexingService elasticIndexingService,
            ElasticsearchIndexManager elasticIndexManager
            )
        {
            _elasticIndexManager = elasticIndexManager;
            _elasticIndexingService = elasticIndexingService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "elasticsearch-index", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                foreach (var index in indices)
                {
                    var elasticIndexSettings = index.ToObject<Dictionary<string, ElasticsearchIndexSettings>>().FirstOrDefault();

                    if (!await _elasticIndexManager.Exists(elasticIndexSettings.Key))
                    {
                        elasticIndexSettings.Value.IndexName = elasticIndexSettings.Key;
                        await _elasticIndexingService.CreateIndexAsync(elasticIndexSettings.Value);
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
