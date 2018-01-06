using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionByHttpRequestFilterIndex : MapIndex
    {
        public int WorkflowDefinitionId { get; set; }
        public int ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string AreaName { get; set; }
    }

    public class HttpRequestFilterEventIndexProvider : IndexProvider<WorkflowDefinitionRecord>
    {
        public override void Describe(DescribeContext<WorkflowDefinitionRecord> context)
        {
            context.For<WorkflowDefinitionByHttpRequestFilterIndex>()
                .Map(workflowDefinition =>
                    workflowDefinition.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && x.IsStart).Select(x =>
                    {
                        dynamic properties = x.Properties;
                        var httpMethod = (string)properties.HttpMethod;
                        var controllerName = (string)properties.RouteValues.controller;
                        var actionName = (string)properties.RouteValues.action;
                        var areaName = (string)properties.RouteValues.area;
                        var index = new WorkflowDefinitionByHttpRequestFilterIndex
                        {
                            WorkflowDefinitionId = workflowDefinition.Id,
                            ActivityId = x.Id,
                            HttpMethod = httpMethod,
                            ControllerName = controllerName,
                            ActionName = actionName,
                            AreaName = areaName
                        };

                        return index;
                    })
                );
        }
    }
}