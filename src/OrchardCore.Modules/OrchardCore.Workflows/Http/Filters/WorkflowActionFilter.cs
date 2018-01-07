using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Filters
{
    public class WorkflowActionFilter : IAsyncActionFilter
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRouteEntries _workflowDefinitionRouteEntries;
        private readonly IWorkflowInstanceRouteEntries _workflowInstanceRouteEntries;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;

        public WorkflowActionFilter(
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRouteEntries workflowDefinitionRouteEntries,
            IWorkflowInstanceRouteEntries workflowInstanceRouteEntries,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowinstanceRepository
        )
        {
            _workflowManager = workflowManager;
            _workflowDefinitionRouteEntries = workflowDefinitionRouteEntries;
            _workflowInstanceRouteEntries = workflowInstanceRouteEntries;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowInstanceRepository = workflowinstanceRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var routeValues = context.RouteData.Values;
            var workflowDefinitionEntries = _workflowDefinitionRouteEntries.GetWorkflowRouteEntries(httpMethod, routeValues);
            var workflowInstanceEntries = _workflowInstanceRouteEntries.GetWorkflowRouteEntries(httpMethod, routeValues);

            if (workflowDefinitionEntries.Any())
            {
                var workflowDefinitionIds = workflowDefinitionEntries.Select(x => int.Parse(x.WorkflowId)).ToList();
                var workflowDefinitions = (await _workflowDefinitionRepository.GetWorkflowDefinitionsAsync(workflowDefinitionIds)).ToDictionary(x => x.Id);

                foreach (var entry in workflowDefinitionEntries)
                {
                    var workflowDefinition = workflowDefinitions[int.Parse(entry.WorkflowId)];
                    var activity = workflowDefinition.Activities.Single(x => x.Id == entry.ActivityId);
                    await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);
                }
            }

            if (workflowInstanceEntries.Any())
            {
                var workflowInstanceUid = context.HttpContext.Request.Query["uid"];
                var correlationId = context.HttpContext.Request.Query["correlationId"];
                var query = workflowInstanceEntries;

                if (!string.IsNullOrWhiteSpace(workflowInstanceUid))
                {
                    query = query.Where(x => x.WorkflowId == workflowInstanceUid);
                }

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    query = query.Where(x => string.Equals(x.CorrelationId, correlationId, StringComparison.OrdinalIgnoreCase));
                }

                var filteredWorkflowInstanceEntries = query.ToList();
                var workflowInstanceUids = filteredWorkflowInstanceEntries.Select(x => x.WorkflowId).ToList();
                var workflowInstances = (await _workflowInstanceRepository.GetAsync(workflowInstanceUids)).ToDictionary(x => x.Uid);

                foreach (var entry in filteredWorkflowInstanceEntries)
                {
                    var workflowInstance = workflowInstances[entry.WorkflowId];
                    var awaitingActivity = workflowInstance.AwaitingActivities.First(x => x.ActivityId == entry.ActivityId);
                    await _workflowManager.ResumeWorkflowAsync(workflowInstance, awaitingActivity);
                }
            }

            await next();
        }
    }
}
