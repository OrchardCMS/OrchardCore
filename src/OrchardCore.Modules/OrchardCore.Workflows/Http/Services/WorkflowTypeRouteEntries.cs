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
    internal class WorkflowTypeRouteEntries : WorkflowRouteEntries<WorkflowTypeRouteDocument>, IWorkflowTypeRouteEntries
    {
        public WorkflowTypeRouteEntries(IVolatileDocumentManager<WorkflowTypeRouteDocument> documentManager) : base(documentManager) { }

        protected override async Task<WorkflowTypeRouteDocument> CreateDocumentAsync()
        {
            var workflowTypeDictionary = (await Session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            var workflowTypeRouteEntries =
                from workflowType in workflowTypeDictionary.Values
                from entry in GetWorkflowTypeRoutesEntries(workflowType, ActivityLibrary)
                select entry;

            var document = new WorkflowTypeRouteDocument();
            AddEntries(document, workflowTypeRouteEntries);

            return document;
        }

        internal static IEnumerable<WorkflowRoutesEntry> GetWorkflowTypeRoutesEntries(WorkflowType workflowType, IActivityLibrary activityLibrary)
        {
            return workflowType.Activities.Where(x => x.IsStart && x.Name == HttpRequestFilterEvent.EventName).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowType.Id.ToString(),
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues
                };

                return entry;
            });
        }
    }
}
