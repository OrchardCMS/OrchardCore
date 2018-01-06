using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowDefinitionRouteEntries : WorkflowRouteEntriesBase, IWorkflowDefinitionRouteEntries
    {
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowDefinitionRoutesEntries(WorkflowDefinitionRecord workflowDefinition, IActivityLibrary activityLibrary)
        {
            return workflowDefinition.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && x.IsStart).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowDefinition.Id.ToString(),
                    ActivityId = x.Id,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues
                };

                return entry;
            });
        }
    }
}
