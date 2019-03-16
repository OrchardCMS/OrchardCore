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
    internal class WorkflowTypeRoutesProvider : IWorkflowRoutesProvider
    {
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowTypeRouteEntries _workflowTypeRouteEntries;
        private readonly IActivityLibrary _activityLibrary;

        public WorkflowTypeRoutesProvider(IWorkflowTypeStore workflowTypeStore, IActivityLibrary activityLibrary, IWorkflowTypeRouteEntries workflowTypeRouteEntries)
        {
            _activityLibrary = activityLibrary;
            _workflowTypeStore = workflowTypeStore;
            _workflowTypeRouteEntries = workflowTypeRouteEntries;
        }

        public async Task RegisterRoutesAsync()
        {
            var workflowTypes = await _workflowTypeStore.ListAsync();

            var workflowTypeRouteEntryQuery =
                from workflowType in workflowTypes
                from entry in WorkflowTypeRouteEntries.GetWorkflowTypeRoutesEntries(workflowType, _activityLibrary)
                select entry;

            _workflowTypeRouteEntries.AddEntries(workflowTypeRouteEntryQuery);
        }
    }
}
