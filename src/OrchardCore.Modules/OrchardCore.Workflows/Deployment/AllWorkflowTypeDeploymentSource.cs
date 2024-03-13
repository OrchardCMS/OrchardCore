using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Deployment
{
    public class AllWorkflowTypeDeploymentSource : IDeploymentSource
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllWorkflowTypeDeploymentSource(
            IWorkflowTypeStore workflowTypeStore,
            IOptions<ContentSerializerJsonOptions> jsonSerializerOptions)
        {
            _workflowTypeStore = workflowTypeStore;
            _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllWorkflowTypeDeploymentStep)
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
                var objectData = JObject.FromObject(workflow, _jsonSerializerOptions);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql
                objectData.Remove(nameof(workflow.Id));
                data.Add(objectData);
            }

            return;
        }
    }
}
