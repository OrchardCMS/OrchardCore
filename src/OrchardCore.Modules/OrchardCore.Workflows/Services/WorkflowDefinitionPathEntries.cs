using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowDefinitionPathEntries : WorkflowPathEntriesBase, IWorkflowDefinitionPathEntries
    {
        public static IEnumerable<WorkflowPathEntry> GetWorkflowPathEntries(WorkflowDefinitionRecord workflowDefinition, IActivityLibrary activityLibrary)
        {
            return workflowDefinition.Activities.Where(x => x.Name == HttpRequestEvent.EventName && x.IsStart).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestEvent>(x);
                var entry = new WorkflowPathEntry
                {
                    WorkflowId = workflowDefinition.Id.ToString(),
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    Path = activity.RequestPath
                };

                return entry;
            });
        }
    }
}
