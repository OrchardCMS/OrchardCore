using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services
{
    public class WorkflowInstanceRouteEntries : WorkflowRouteEntriesBase, IWorkflowInstanceRouteEntries
    {
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowInstanceRoutesEntries(WorkflowInstanceRecord workflowInstance, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstance.AwaitingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            var workflowDefinition = workflowInstance.WorkflowDefinition;
            return workflowDefinition.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && awaitingActivityIds.ContainsKey(x.Id)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowInstance.Uid,
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues,
                    CorrelationId = workflowInstance.CorrelationId
                };

                return entry;
            });
        }
    }
}
