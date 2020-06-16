using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services
{
    internal class WorkflowRouteEntries : WorkflowRouteEntriesBase, IWorkflowInstanceRouteEntries
    {
        internal static IEnumerable<WorkflowRoutesEntry> GetWorkflowRoutesEntries(WorkflowType workflowType, Workflow workflow, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflow.BlockingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowType.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && awaitingActivityIds.ContainsKey(x.ActivityId)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflow.WorkflowId,
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues,
                };

                return entry;
            });
        }
    }
}
