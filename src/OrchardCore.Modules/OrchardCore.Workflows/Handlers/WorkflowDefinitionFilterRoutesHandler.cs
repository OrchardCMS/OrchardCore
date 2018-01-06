using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers
{
    public class WorkflowDefinitionRoutesHandler : WorkflowDefinitionHandlerBase
    {
        private readonly IWorkflowRouteEntries _workflowRouteEntries;
        private readonly IWorkflowPathEntries _workflowPathEntries;

        public WorkflowDefinitionRoutesHandler(IWorkflowRouteEntries workflowRouteEntries, IWorkflowPathEntries workflowPathEntries)
        {
            _workflowRouteEntries = workflowRouteEntries;
            _workflowPathEntries = workflowPathEntries;
        }

        public override Task CreatedAsync(WorkflowDefinitionCreatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowDefinitionUpdatedContext context)
        {
            UpdateRouteEntries(context);
            UpdatePathEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowDefinitionDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowDefinition.Id);
            _workflowPathEntries.RemoveEntries(context.WorkflowDefinition.Id);
            return Task.CompletedTask;
        }

        private void UpdateRouteEntries(WorkflowDefinitionContext context)
        {
            var workflowDefinition = context.WorkflowDefinition;
            var entries = workflowDefinition.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && x.IsStart).Select(x =>
            {
                dynamic properties = x.Properties;
                var httpMethod = (string)properties.HttpMethod;
                var controllerName = (string)properties.RouteValues.controller;
                var actionName = (string)properties.RouteValues.action;
                var areaName = (string)properties.RouteValues.area;
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowDefinitionId = workflowDefinition.Id,
                    ActivityId = x.Id,
                    HttpMethod = httpMethod,
                    RouteValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary
                    {
                        { "controller", controllerName },
                        { "action", actionName },
                        { "area", areaName }
                    }
                };

                return entry;
            });

            _workflowRouteEntries.AddEntries(workflowDefinition.Id, entries);
        }

        private void UpdatePathEntries(WorkflowDefinitionContext context)
        {
            var workflowDefinition = context.WorkflowDefinition;
            var entries = workflowDefinition.Activities.Where(x => x.Name == HttpRequestEvent.EventName && x.IsStart).Select(x =>
            {
                dynamic properties = x.Properties;
                var httpMethod = (string)properties.HttpMethod;
                var path = (string)properties.RequestPath;
                var entry = new WorkflowPathEntry
                {
                    WorkflowDefinitionId = workflowDefinition.Id,
                    ActivityId = x.Id,
                    HttpMethod = httpMethod,
                    Path = path
                };

                return entry;
            });

            _workflowPathEntries.AddEntries(workflowDefinition.Id, entries);
        }
    }
}
