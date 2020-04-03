using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Modules;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using YesSql.Services;

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

            int skip = 0;
            int pageSize = 50;

            var workflowTypeDictionary = (await _workflowTypeStore.ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            var workflowTypeRouteEntryQuery =
                from workflowType in workflowTypeDictionary.Values
                from entry in WorkflowTypeRouteEntries.GetWorkflowTypeRoutesEntries(workflowType, _activityLibrary)
                select entry;

            _workflowTypeRouteEntries.AddEntries(workflowTypeRouteEntryQuery);

            while (true)
            {
                // TODO: Clear the session when the feature is available
                // Right now the identity map is keep even after CommitAsync() is invoked
                //using (var session = _store.CreateSession())
                //{
                    var query = await _session
                        .QueryIndex<WorkflowBlockingActivitiesIndex>(index =>
                        index.ActivityName == HttpRequestFilterEvent.EventName)
                        .Skip(skip)
                        .Take(pageSize)
                        .ListAsync();

                    if (!query.Any())
                    {
                        break;
                    }

                    skip += pageSize;
                    var pendingWorkflowIndexes = query.ToList();
                    var pendingWorkflowIds = pendingWorkflowIndexes.Select(x => x.WorkflowId).Distinct().ToArray();
                    var pendingWorkflows = await _session.Query<Workflow, WorkflowIndex>(x => x.WorkflowId.IsIn(pendingWorkflowIds)).ListAsync();

                    var workflowRouteEntryQuery =
                        from workflow in pendingWorkflows
                        from entry in WorkflowRouteEntries.GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, _activityLibrary)
                        select entry;

                    _workflowInstanceRouteEntries.AddEntries(workflowRouteEntryQuery);
                //}
            }
        }

        public override async Task ActivatingAsync()
        {
            await RegisterRoutesAsync();
        }
    }
}
