using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

            var data = new JsonArray();
            result.Steps.Add(new JsonObject
            {
                ["name"] = "WorkflowType",
                ["data"] = data,
            });

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
