using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Locking.Distributed;
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
        private readonly IDistributedLock _distributedLock;

        public WorkflowActionFilter(
            IWorkflowManager workflowManager,
            IWorkflowTypeRouteEntries workflowTypeRouteEntries,
            IWorkflowInstanceRouteEntries workflowRouteEntries,
            IWorkflowTypeStore workflowTypeStore,
            IWorkflowStore workflowStore,
            IDistributedLock distributedLock
        )
        {
            _workflowManager = workflowManager;
            _workflowTypeRouteEntries = workflowTypeRouteEntries;
            _workflowRouteEntries = workflowRouteEntries;
            _workflowTypeStore = workflowTypeStore;
            _workflowStore = workflowStore;
            _distributedLock = distributedLock;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var routeValues = context.RouteData.Values;
            var workflowTypeEntries = await _workflowTypeRouteEntries.GetWorkflowRouteEntriesAsync(httpMethod, routeValues);
            var workflowEntries = await _workflowRouteEntries.GetWorkflowRouteEntriesAsync(httpMethod, routeValues);

            if (workflowTypeEntries.Any())
            {
                var workflowTypeIds = workflowTypeEntries.Select(x => Int64.Parse(x.WorkflowId)).ToList();
                var workflowTypes = (await _workflowTypeStore.GetAsync(workflowTypeIds)).ToDictionary(x => x.Id);
                var correlationId = routeValues.GetValue<string>("correlationid");

                foreach (var entry in workflowTypeEntries)
                {
                    if (workflowTypes.TryGetValue(Int64.Parse(entry.WorkflowId), out var workflowType))
                    {
                        var activity = workflowType.Activities.Single(x => x.ActivityId == entry.ActivityId);

                        if (activity.IsStart)
                        {
                            // If a singleton, try to acquire a lock per workflow type.
                            (var locker, var locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType);
                            if (!locked)
                            {
                                continue;
                            }

                            await using var acquiredLock = locker;

                            // Check if this is a workflow singleton and there's already an halted instance on any activity.
                            if (workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
                            {
                                continue;
                            }

                            await _workflowManager.StartWorkflowAsync(workflowType, activity, null, correlationId);
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
                        // If atomic, try to acquire a lock per workflow instance.
                        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowLockAsync(workflow);
                        if (!locked)
                        {
                            continue;
                        }

                        await using var acquiredLock = locker;

                        // If atomic, check if the workflow still exists and is still correlated.
                        var haltedWorkflow = workflow.IsAtomic ? await _workflowStore.GetAsync(workflow.Id) : workflow;
                        if (haltedWorkflow == null || (!String.IsNullOrWhiteSpace(correlationId) && haltedWorkflow.CorrelationId != correlationId))
                        {
                            continue;
                        }

                        // And if it is still halted on this activity.
                        var blockingActivity = haltedWorkflow.BlockingActivities.SingleOrDefault(x => x.ActivityId == entry.ActivityId);
                        if (blockingActivity != null)
                        {
                            await _workflowManager.ResumeWorkflowAsync(haltedWorkflow, blockingActivity);
                        }
                    }
                }
            }

            await next();
        }
    }
}
