using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services
{
    public class WorkflowInstancePathEntries : WorkflowPathEntriesBase, IWorkflowInstancePathEntries
    {
        public static IEnumerable<WorkflowPathEntry> GetWorkflowPathEntries(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstanceRecord.AwaitingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowDefinitionRecord.Activities.Where(x => x.Name == HttpRequestEvent.EventName && awaitingActivityIds.ContainsKey(x.Id)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestEvent>(x);
                var entry = new WorkflowPathEntry
                {
                    WorkflowId = workflowInstanceRecord.Uid,
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    Path = activity.RequestPath,
                    CorrelationId = workflowInstanceRecord.CorrelationId
                };

                return entry;
            });
        }
    }
}
