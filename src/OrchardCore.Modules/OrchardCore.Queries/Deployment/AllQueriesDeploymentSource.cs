using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;

namespace OrchardCore.Queries.Deployment
{
    public class AllQueriesDeploymentSource : IDeploymentSource
    {
        private readonly IQueryManager _queryManager;

        public AllQueriesDeploymentSource(IQueryManager queryManager)
        {
            _queryManager = queryManager;
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
                ["Queries"] = JArray.FromObject(queries),
            });
        }
    }
}
