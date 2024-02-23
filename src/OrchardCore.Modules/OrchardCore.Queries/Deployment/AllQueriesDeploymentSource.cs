using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment
{
    public class AllQueriesDeploymentSource : IDeploymentSource
    {
        private readonly IQueryManager _queryManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllQueriesDeploymentSource(
            IQueryManager queryManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _queryManager = queryManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allQueriesStep = step as AllQueriesDeploymentStep;

            if (allQueriesStep == null)
            {
                return;
            }

            var queries = await _queryManager.ListQueriesAsync();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Queries",
                ["Queries"] = JArray.FromObject(queries, _jsonSerializerOptions),
            });
        }
    }
}
