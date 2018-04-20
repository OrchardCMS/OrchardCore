using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services
{
    public class WorkflowDefinitionRouteEntries : WorkflowRouteEntriesBase, IWorkflowDefinitionRouteEntries
    {
        public static IEnumerable<WorkflowRoutesEntry> GetWorkflowDefinitionRoutesEntries(WorkflowDefinition workflowDefinition, IActivityLibrary activityLibrary)
        {
            return workflowDefinition.Activities.Where(x => x.IsStart && x.Name == HttpRequestFilterEvent.EventName).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowDefinition.Id.ToString(),
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues
                };

                return entry;
            });
        }
    }
}
