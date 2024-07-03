using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace OrchardCore.Queries.Recipes
{
    /// <summary>
    /// This recipe step creates a set of queries.
    /// </summary>
    public class QueryStep : IRecipeStepHandler
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogger _logger;

        public QueryStep(
            ISession session,
            IServiceProvider serviceProvider,
            IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
            ILogger<QueryStep> logger)
        {
            _session = session;
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

            foreach (var token in model.Queries.Cast<JsonObject>())
            {
                var sourceName = token[nameof(Query.Source)].ToString();
                var sample = _serviceProvider.GetKeyedService<IQuerySource>(sourceName)?.Create();

                if (sample == null)
                {
                    _logger.LogError("Could not find query source: '{QuerySource}'. The query '{QueryName}' will not be imported.", sourceName, token[nameof(Query.Name)].ToString());

                    continue;
                }

                var query = token.ToObject(sample.GetType(), _jsonSerializerOptions) as Query;
                await _session.SaveAsync(query);
            }

            await _session.SaveChangesAsync();
        }
    }

    public class QueryStepModel
    {
        public JsonArray Queries { get; set; }
    }
}
