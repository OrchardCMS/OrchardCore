using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services
{
    internal class WorkflowInstanceRouteEntries : WorkflowRouteEntries<WorkflowRouteDocument>, IWorkflowInstanceRouteEntries
    {
        public WorkflowInstanceRouteEntries(IVolatileDocumentManager<WorkflowRouteDocument> documentManager) : base(documentManager) { }

        protected override async Task<WorkflowRouteDocument> CreateDocumentAsync()
        {
            var workflowTypeDictionary = (await Session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToDictionary(x => x.WorkflowTypeId);

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

                if (!pendingWorkflows.Any())
                {
                    break;
                }

                var workflowRouteEntries =
                    from workflow in pendingWorkflows
                    from entry in GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, ActivityLibrary)
                    select entry;

                AddEntries(document, workflowRouteEntries);

                if (pendingWorkflows.Count() < pageSize)
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
}
