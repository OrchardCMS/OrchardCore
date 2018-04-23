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
        private readonly IWorkflowTypeStore _workflowDefinitionRepository;
        private readonly IWorkflowStore _workflowInstanceRepository;

        public WorkflowActionFilter(
            IWorkflowManager workflowManager,
            IWorkflowDefinitionRouteEntries workflowDefinitionRouteEntries,
            IWorkflowInstanceRouteEntries workflowInstanceRouteEntries,
            IWorkflowTypeStore workflowDefinitionRepository,
            IWorkflowStore workflowinstanceRepository
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
                var workflowDefinitionIds = workflowDefinitionEntries.Select(x => Int32.Parse(x.WorkflowId)).ToList();
                var workflowDefinitions = (await _workflowDefinitionRepository.GetAsync(workflowDefinitionIds)).ToDictionary(x => x.Id);

                foreach (var entry in workflowDefinitionEntries)
                {
                    var workflowDefinition = workflowDefinitions[Int32.Parse(entry.WorkflowId)];
                    var activity = workflowDefinition.Activities.Single(x => x.ActivityId == entry.ActivityId);
                    await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);
                }
            }

            await next();
        }
    }
}
