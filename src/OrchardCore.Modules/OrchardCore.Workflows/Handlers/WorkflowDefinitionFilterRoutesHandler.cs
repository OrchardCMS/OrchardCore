using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers
{
    public class WorkflowDefinitionFilterRoutesHandler : WorkflowDefinitionHandlerBase
    {
        private readonly IWorkflowRouteEntries _workflowRouteEntries;

        public WorkflowDefinitionFilterRoutesHandler(IWorkflowRouteEntries workflowRouteEntries)
        {
            _workflowRouteEntries = workflowRouteEntries;
        }

        public override Task CreatedAsync(WorkflowDefinitionCreatedContext context)
        {
            UpdateEntries(context);
            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(WorkflowDefinitionUpdatedContext context)
        {
            UpdateEntries(context);
            return Task.CompletedTask;
        }

        public override Task DeletedAsync(WorkflowDefinitionDeletedContext context)
        {
            _workflowRouteEntries.RemoveEntries(context.WorkflowDefinition.Id);
            return Task.CompletedTask;
        }

        private void UpdateEntries(WorkflowDefinitionContext context)
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
    }
}
