using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Queries.Recipes
{
    /// <summary>
    /// This recipe step creates a set of queries.
    /// </summary>
    public class QueryStep : IRecipeStepHandler
    {
        private readonly IQueryManager _queryManager;

        public QueryStep(IQueryManager queryManager)
        {
            _queryManager = queryManager;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Query", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var model = context.Step.ToObject<QueryStepModel>();

            foreach(JObject token in model.Data)
            {
                var query = token.ToObject<Query>();
                _queryManager.SaveQueryAsync(query);
            }

            return Task.CompletedTask;  
        }
    }

    public class QueryStepModel
    {
        public JArray Data { get; set; }
    }
}