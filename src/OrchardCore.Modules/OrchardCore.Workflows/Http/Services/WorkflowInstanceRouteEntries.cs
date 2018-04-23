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
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowInstanceRoutesEntries(WorkflowType workflowDefinitionRecord, Workflow workflowInstanceRecord, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstanceRecord.BlockingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowDefinitionRecord.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && awaitingActivityIds.ContainsKey(x.ActivityId)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowInstanceRecord.WorkflowId,
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues,
                    CorrelationId = workflowInstanceRecord.CorrelationId
                };

                return entry;
            });
        }
    }
}
