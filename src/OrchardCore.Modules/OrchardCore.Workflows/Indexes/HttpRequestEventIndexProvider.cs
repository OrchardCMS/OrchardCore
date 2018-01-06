using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowDefinitionByHttpRequestIndex : MapIndex
    {
        public int WorkflowDefinitionId { get; set; }
        public int ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public string RequestPath { get; set; }
    }

    public class HttpRequestEventIndexProvider : IndexProvider<WorkflowDefinitionRecord>
    {
        public override void Describe(DescribeContext<WorkflowDefinitionRecord> context)
        {
            context.For<WorkflowDefinitionByHttpRequestIndex>()
                .Map(workflowDefinition =>
                    workflowDefinition.Activities.Where(x => x.Name == HttpRequestEvent.EventName && x.IsStart).Select(x =>
                    {
                        dynamic properties = x.Properties;
                        var httpMethod = (string)properties.HttpMethod;
                        var requestPath = (string)properties.RequestPath;
                        var index = new WorkflowDefinitionByHttpRequestIndex
                        {
                            WorkflowDefinitionId = workflowDefinition.Id,
                            ActivityId = x.Id,
                            HttpMethod = httpMethod,
                            RequestPath = requestPath
                        };

                        return index;
                    })
                );
        }
    }
}