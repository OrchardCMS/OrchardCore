using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services;

internal sealed class WorkflowInstanceRouteEntries : WorkflowRouteEntries<WorkflowRouteDocument>, IWorkflowInstanceRouteEntries
{
    public WorkflowInstanceRouteEntries(IVolatileDocumentManager<WorkflowRouteDocument> documentManager) : base(documentManager) { }

    protected override async Task<WorkflowRouteDocument> CreateDocumentAsync()
    {
        var workflowTypeDictionary = (await Session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToDictionary(x => x.WorkflowTypeId);
        var versionStore = ShellScope.Services.GetRequiredService<IWorkflowTypeVersionStore>();

        var skip = 0;
        var pageSize = 50;
        var document = new WorkflowRouteDocument();

        while (true)
        {
            var pendingWorkflows = await Session
                .Query<Workflow, WorkflowBlockingActivitiesIndex>(index =>
                    index.ActivityName == HttpRequestFilterEvent.EventName)
                .Skip(skip)
                .Take(pageSize)
                .ListAsync();

            if (pendingWorkflows.Count == 0)
            {
                break;
            }

            foreach (var workflow in pendingWorkflows)
            {
                if (!workflowTypeDictionary.TryGetValue(workflow.WorkflowTypeId, out var workflowType))
                {
                    continue;
                }

                // The routes an instance waits on are those of the version it runs.
                var definition = await versionStore.GetWorkflowTypeAsync(workflowType, workflow.WorkflowTypeVersionId);
                AddEntries(document, GetWorkflowRoutesEntries(definition, workflow, ActivityLibrary));
            }

            if (pendingWorkflows.Count < pageSize)
            {
                break;
            }

            skip += pageSize;
        }

        return document;
    }

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
