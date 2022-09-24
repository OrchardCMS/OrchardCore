using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Queries.Recipes
{
    /// <summary>
    /// This recipe step creates a set of queries.
    /// </summary>
    public class QueryStep : IRecipeStepHandler
    {
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<IQuerySource> _querySources;
        private readonly ILogger _logger;

        public QueryStep(
            IQueryManager queryManager,
            IEnumerable<IQuerySource> querySources,
            ILogger<QueryStep> logger)
        {
            _queryManager = queryManager;
            _querySources = querySources;
            _logger = logger;
        }

        public int Order => 0;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Queries", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<QueryStepModel>();

            foreach (JObject token in model.Queries)
            {
                var sourceName = token[nameof(Query.Source)].ToString();
                var sample = _querySources.FirstOrDefault(x => x.Name == sourceName)?.Create();

                if (sample == null)
                {
                    _logger.LogError("Could not find query source: '{QuerySource}'. The query '{QueryName}' will not be imported.", sourceName, token[nameof(Query.Name)].ToString());

                    continue;
                }

                var query = token.ToObject(sample.GetType()) as Query;
                await _queryManager.SaveQueryAsync(query.Name, query);
            }
        }
    }

    public class QueryStepModel
    {
        public JArray Queries { get; set; }
    }
}
