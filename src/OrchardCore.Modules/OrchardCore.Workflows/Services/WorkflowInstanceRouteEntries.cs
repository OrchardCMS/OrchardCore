using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstanceRouteEntries : WorkflowRouteEntriesBase, IWorkflowInstanceRouteEntries
    {
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowInstanceRoutesEntries(WorkflowInstanceRecord workflowInstance, WorkflowDefinitionRecord workflowDefinition, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstance.AwaitingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
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
