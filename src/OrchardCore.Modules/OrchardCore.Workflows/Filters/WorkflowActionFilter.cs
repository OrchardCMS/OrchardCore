using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
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
            var requestPath = context.HttpContext.Request.Path.Value;
            var indexes = await _session.QueryIndex<WorkflowDefinitionByHttpRequestIndex>(x => x.HttpMethod == httpMethod && x.RequestPath == requestPath).ListAsync();
            var ids = indexes.Select(x => x.WorkflowDefinitionId).ToList();
            var workflowDefinitions = await _workflowDefinitionRepository.GetWorkflowDefinitionsAsync(ids);

            foreach (var workflowDefinition in workflowDefinitions)
            {
                await _workflowManager.StartWorkflowAsync(workflowDefinition);
            }

            await next();
        }
    }
}
