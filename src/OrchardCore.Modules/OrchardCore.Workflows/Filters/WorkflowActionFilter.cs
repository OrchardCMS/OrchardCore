using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Filters
{
    public class WorkflowActionFilter : IAsyncActionFilter
    {
        private readonly ISession _session;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowRouteEntries _workflowRouteEntries;

        public WorkflowActionFilter(ISession session, IWorkflowManager workflowManager, IWorkflowRouteEntries workflowRouteEntries, IWorkflowDefinitionRepository workflowDefinitionRepository)
        {
            _session = session;
            _workflowManager = workflowManager;
            _workflowRouteEntries = workflowRouteEntries;
            _workflowDefinitionRepository = workflowDefinitionRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var routeValues = context.HttpContext.GetRouteData().Values;
            var entries = _workflowRouteEntries.GetWorkflowRouteEntries(httpMethod, routeValues);

            if (entries.Any())
            {
                var workflowDefinitionIds = entries.Select(x => x.WorkflowDefinitionId).ToList();
                var workflowDefinitions = (await _workflowDefinitionRepository.GetWorkflowDefinitionsAsync(workflowDefinitionIds)).ToDictionary(x => x.Id);

                foreach (var entry in entries)
                {
                    var workflowDefinition = workflowDefinitions[entry.WorkflowDefinitionId];
                    var activity = workflowDefinition.Activities.Single(x => x.Id == entry.ActivityId);
                    await _workflowManager.StartWorkflowAsync(workflowDefinition, activity);
                }
            }

            await next();
        }
    }
}
