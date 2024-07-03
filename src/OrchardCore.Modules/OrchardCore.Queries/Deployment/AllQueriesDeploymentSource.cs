using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Queries.Indexes;
using YesSql;

namespace OrchardCore.Queries.Deployment
{
    public class AllQueriesDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllQueriesDeploymentSource(
            ISession session,
            IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
        {
            _session = session;
            _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allQueriesStep = step as AllQueriesDeploymentStep;

            if (allQueriesStep == null)
            {
                return;
            }

            var queries = await _session.Query<Query, QueryIndex>().ListAsync();

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Queries",
                ["Queries"] = JArray.FromObject(queries, _jsonSerializerOptions),
            });
        }
    }
}
