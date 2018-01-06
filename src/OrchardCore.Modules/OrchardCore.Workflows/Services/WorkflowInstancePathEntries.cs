using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowInstancePathEntries : WorkflowPathEntriesBase, IWorkflowInstancePathEntries
    {
        public static IEnumerable<WorkflowPathEntry> GetWorkflowPathEntries(WorkflowInstanceRecord workflowInstance, WorkflowDefinitionRecord workflowDefinition, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflowInstance.AwaitingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowDefinition.Activities.Where(x => x.Name == HttpRequestEvent.EventName && awaitingActivityIds.ContainsKey(x.Id)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestEvent>(x);
                var entry = new WorkflowPathEntry
                {
                    WorkflowId = workflowInstance.Uid,
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    Path = activity.RequestPath,
                    CorrelationId = workflowInstance.CorrelationId
                };

                return entry;
            });
        }
    }
}
