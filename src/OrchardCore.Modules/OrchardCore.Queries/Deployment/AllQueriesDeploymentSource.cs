using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

            result.Steps.Add(new JObject(
                new JProperty("name", "Queries"),
                new JProperty("Queries", queries.Select(JObject.FromObject))
            ));
        }
    }
}
