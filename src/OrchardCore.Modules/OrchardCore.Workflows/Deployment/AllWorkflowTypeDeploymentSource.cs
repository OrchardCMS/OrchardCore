using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Deployment
{
    public class AllWorkflowTypeDeploymentSource : IDeploymentSource
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public AllWorkflowTypeDeploymentSource(IWorkflowTypeStore workflowTypeStore)
        {
            _workflowTypeStore = workflowTypeStore;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allContentStep = step as AllWorkflowTypeDeploymentStep;

            if (allContentStep == null)
            {
                return;
            }

            var data = new JArray();
            result.Steps.Add(new JObject(
                new JProperty("name", "WorkflowType"),
                new JProperty("data", data)
            ));

            foreach (var workflow in await _workflowTypeStore.ListAsync())
            {
                var objectData = JObject.FromObject(workflow);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql
                objectData.Remove(nameof(workflow.Id));
                data.Add(objectData);
            }

            return;
        }
    }
}
