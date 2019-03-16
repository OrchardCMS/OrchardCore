using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Services;
using YesSql;
using OrchardCore.Workflows.Models;
using YesSql.Services;

namespace OrchardCore.Workflows.Http.Services
{
    internal class HttpRequestFilterRouteProvider : IWorkflowRoutesProvider
    {
        private readonly IStore _store;
        private readonly IWorkflowInstanceRouteEntries _workflowInstanceRouteEntries;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IActivityLibrary _activityLibrary;

        public HttpRequestFilterRouteProvider(IStore store, IWorkflowInstanceRouteEntries workflowInstanceRouteEntries, IWorkflowTypeStore workflowTypeStore, IActivityLibrary activityLibrary)
        {
            _store = store;
            _activityLibrary = activityLibrary;
            _workflowTypeStore = workflowTypeStore;
            _workflowInstanceRouteEntries = workflowInstanceRouteEntries;
        }

        public async Task RegisterRoutesAsync()
        {
            int skip = 0;
            int pageSize = 50;

            var workflowTypeDictionary = (await _workflowTypeStore.ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            while (true)
            {
                using (var session = _store.CreateSession())
                {
                    var query = await session
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
                    var pendingWorkflows = await session.Query<Workflow, WorkflowIndex>(x => x.WorkflowId.IsIn(pendingWorkflowIds)).ListAsync();

                    var workflowRouteEntryQuery =
                        from workflow in pendingWorkflows
                        from entry in WorkflowRouteEntries.GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, _activityLibrary)
                        select entry;

                    _workflowInstanceRouteEntries.AddEntries(workflowRouteEntryQuery);
                }
            }
        }
    }
}
