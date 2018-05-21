using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Filters
{
    internal class WorkflowActionFilter : IAsyncActionFilter
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowTypeRouteEntries _workflowTypeRouteEntries;
        private readonly IWorkflowInstanceRouteEntries _workflowRouteEntries;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowStore _workflowStore;

        public WorkflowActionFilter(
            IWorkflowManager workflowManager,
            IWorkflowTypeRouteEntries workflowTypeRouteEntries,
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowTypeStore workflowTypeStore,
            IWorkflowStore workflowStore
        )
        {
            _workflowManager = workflowManager;
            _workflowTypeRouteEntries = workflowTypeRouteEntries;
            _workflowRouteEntries = workflowRouteEntries;
            _workflowTypeStore = workflowTypeStore;
            _workflowStore = workflowStore;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var routeValues = context.RouteData.Values;
            var workflowTypeEntries = _workflowTypeRouteEntries.GetWorkflowRouteEntries(httpMethod, routeValues);
            var workflowEntries = _workflowRouteEntries.GetWorkflowRouteEntries(httpMethod, routeValues);

            if (workflowTypeEntries.Any())
            {
                var workflowTypeIds = workflowTypeEntries.Select(x => Int32.Parse(x.WorkflowId)).ToList();
                var workflowTypes = (await _workflowTypeStore.GetAsync(workflowTypeIds)).ToDictionary(x => x.Id);

                foreach (var entry in workflowTypeEntries)
                {
                    var workflowType = workflowTypes[Int32.Parse(entry.WorkflowId)];
                    var activity = workflowType.Activities.Single(x => x.ActivityId == entry.ActivityId);
                    await _workflowManager.StartWorkflowAsync(workflowType, activity);
                }
            }

            await next();
        }
    }
}
