using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Filters
{
    public class WorkflowActionFilter : IAsyncActionFilter
    {
        private readonly ISession _session;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;

        public WorkflowActionFilter(ISession session, IWorkflowManager workflowManager, IWorkflowDefinitionRepository workflowDefinitionRepository)
        {
            _session = session;
            _workflowManager = workflowManager;
            _workflowDefinitionRepository = workflowDefinitionRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var routeValues = context.HttpContext.GetRouteData();
            var controllerName = routeValues.Values.GetValue<string>("controller");
            var actionName = routeValues.Values.GetValue<string>("action");
            var areaName = routeValues.Values.GetValue<string>("area");

            // TODO: Cache workflow definition IDs by HTTP methods and paths.
            var indexes = (await _session.QueryIndex<WorkflowDefinitionByHttpRequestFilterIndex>(x =>
                x.HttpMethod == httpMethod
                && (x.ControllerName == controllerName || x.ControllerName == null || x.ControllerName == "")
                && (x.ActionName == actionName || x.ActionName == null || x.ActionName == "")
                && (x.AreaName == areaName || x.AreaName == null | x.AreaName == "")
            ).ListAsync()).ToList();

            var workflowDefinitionIds = indexes.Select(x => x.WorkflowDefinitionId).ToList();
            var workflowDefinitions = (await _workflowDefinitionRepository.GetWorkflowDefinitionsAsync(workflowDefinitionIds)).ToDictionary(x => x.Id);

            foreach (var index in indexes)
            {
                var workflowDefinition = workflowDefinitions[index.WorkflowDefinitionId];
                var activity = workflowDefinition.Activities.Single(x => x.Id == index.ActivityId);
                await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);
            }

            await next();
        }
    }
}
