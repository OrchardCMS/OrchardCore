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
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowInstanceRoutesEntries(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstanceRecord.AwaitingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowDefinitionRecord.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && awaitingActivityIds.ContainsKey(x.Id)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowInstanceRecord.Uid,
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues,
                    CorrelationId = workflowInstanceRecord.CorrelationId
                };

                return entry;
            });
        }
    }
}
