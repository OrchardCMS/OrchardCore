using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Workflows.Helpers;
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
                var correlationId = routeValues.GetValue<string>("correlationid");

                foreach (var entry in workflowTypeEntries)
                {
                    if (workflowTypes.TryGetValue(Int32.Parse(entry.WorkflowId), out var workflowType))
                    {
                        var activity = workflowType.Activities.Single(x => x.ActivityId == entry.ActivityId);

                        if (activity.IsStart)
                        {
                            // If this is not a singleton workflow or there is not already an halted instance, start a new workflow.
                            if (!workflowType.IsSingleton || !await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
                            {
                                await _workflowManager.StartWorkflowAsync(workflowType, activity, null, correlationId);
                            }
                        }
                    }
                }
            }

            if (workflowEntries.Any())
            {
                var workflowIds = workflowEntries.Select(x => x.WorkflowId).ToList();
                var workflows = (await _workflowStore.GetAsync(workflowIds)).ToDictionary(x => x.WorkflowId);
                var correlationId = routeValues.GetValue<string>("correlationid");

                foreach (var entry in workflowEntries)
                {
                    if (workflows.TryGetValue(entry.WorkflowId, out var workflow) &&
                        (String.IsNullOrWhiteSpace(correlationId) ||
                        workflow.CorrelationId == correlationId))
                    {
                        var blockingActivity = workflow.BlockingActivities.Single(x => x.ActivityId == entry.ActivityId);
                        await _workflowManager.ResumeWorkflowAsync(workflow, blockingActivity);
                    }
                }
            }

            await next();
        }
    }
}
