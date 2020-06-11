using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services
{
    internal class HttpRequestRouteActivator : ModularTenantEvents
    {
        private readonly ISession _session;
        private readonly IWorkflowInstanceRouteEntries _workflowInstanceRouteEntries;
        private readonly IWorkflowTypeRouteEntries _workflowTypeRouteEntries;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IActivityLibrary _activityLibrary;

        public HttpRequestRouteActivator(
            ISession session,
            IWorkflowInstanceRouteEntries workflowInstanceRouteEntries,
            IWorkflowTypeRouteEntries workflowTypeRouteEntries,
            IWorkflowTypeStore workflowTypeStore,
            IActivityLibrary activityLibrary)
        {
            _activityLibrary = activityLibrary;
            _workflowTypeStore = workflowTypeStore;
            _session = session;
            _workflowInstanceRouteEntries = workflowInstanceRouteEntries;
            _workflowTypeRouteEntries = workflowTypeRouteEntries;
        }

        private async Task RegisterRoutesAsync()
        {
            // Registers all the routes that running workflow instances are paused on.

            var skip = 0;
            var pageSize = 50;

            var workflowTypeDictionary = (await _workflowTypeStore.ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            var workflowTypeRouteEntryQuery =
                from workflowType in workflowTypeDictionary.Values
                from entry in WorkflowTypeRouteEntries.GetWorkflowTypeRoutesEntries(workflowType, _activityLibrary)
                select entry;

            _workflowTypeRouteEntries.AddEntries(workflowTypeRouteEntryQuery);

            while (true)
            {
                var pendingWorkflows = await _session
                    .Query<Workflow, WorkflowBlockingActivitiesIndex>(index =>
                        index.ActivityName == HttpRequestFilterEvent.EventName)
                    .Skip(skip)
                    .Take(pageSize)
                    .ListAsync();

                if (!pendingWorkflows.Any())
                {
                    break;
                }

                var workflowRouteEntryQuery =
                    from workflow in pendingWorkflows
                    from entry in WorkflowRouteEntries.GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, _activityLibrary)
                    select entry;

                _workflowInstanceRouteEntries.AddEntries(workflowRouteEntryQuery);

                if (pendingWorkflows.Count() < pageSize)
                {
                    break;
                }

                skip += pageSize;
            }
        }

        public override Task ActivatingAsync()
        {
            return RegisterRoutesAsync();
        }
    }
}
