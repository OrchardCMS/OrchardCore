using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Queries.Core;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger _logger;

        public QueryStep(
            IQueryManager queryManager,
            IServiceProvider serviceProvider,
            IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
            ILogger<QueryStep> logger)
        {
            _queryManager = queryManager;
            _serviceProvider = serviceProvider;
            _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Queries", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<QueryStepModel>(_jsonSerializerOptions);
            var queries = new List<Query>();
            foreach (var token in model.Queries.Cast<JsonObject>())
            {
                var name = token[nameof(Query.Name)].ToString();

                if (string.IsNullOrEmpty(name))
                {
                    _logger.LogError("Could not find query name value. The query will not be imported.");

                    continue;
                }

                var sourceName = token[nameof(Query.Source)].ToString();

                if (string.IsNullOrEmpty(sourceName))
                {
                    _logger.LogError("Could not find query source value. The query '{QueryName}' will not be imported.", token[nameof(Query.Name)].ToString());

                    continue;
                }

                var querySource = _serviceProvider.GetKeyedService<IQuerySource>(sourceName);

                if (querySource == null)
                {
                    _logger.LogError("Could not find query source: '{QuerySource}'. The query '{QueryName}' will not be imported.", sourceName, token[nameof(Query.Name)].ToString());

                    continue;
                }

                var query = querySource.Create(token);

                queries.Add(query);
            }

            await _queryManager.SaveQueryAsync(queries.ToArray());
        }
    }

    public class QueryStepModel
    {
        public JsonArray Queries { get; set; }
    }
}
