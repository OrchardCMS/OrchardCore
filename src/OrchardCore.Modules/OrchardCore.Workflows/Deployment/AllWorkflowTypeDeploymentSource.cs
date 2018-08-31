using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Deployment
{
    public class AllWorkflowTypeDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;
        private readonly IWorkflowTypeStore _workflowTypeStore;

        public AllWorkflowTypeDeploymentSource(IWorkflowTypeStore workflowTypeStore, ISession session)
        {
            _workflowTypeStore = workflowTypeStore;
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allContentState = step as AllWorkflowTypeDeploymentStep;

            if (allContentState == null)
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
