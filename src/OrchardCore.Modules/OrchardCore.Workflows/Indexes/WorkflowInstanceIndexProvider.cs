using System;
using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowInstanceIndex : MapIndex
    {
        public string WorkflowDefinitionId { get; set; }
        public string WorkflowInstanceId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class WorkflowInstanceBlockingActivitiesIndex : MapIndex
    {
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public bool ActivityIsStart { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public string WorkflowInstanceId { get; set; }
        public string WorkflowInstanceCorrelationId { get; set; }
    }

    public class WorkflowInstanceIndexProvider : IndexProvider<Workflow>
    {
        public override void Describe(DescribeContext<Workflow> context)
        {
            context.For<WorkflowInstanceIndex>()
                .Map(workflowInstance =>
                    new WorkflowInstanceIndex
                    {
                        WorkflowDefinitionId = workflowInstance.WorkflowTypeId,
                        WorkflowInstanceId = workflowInstance.WorkflowId,
                        CreatedUtc = workflowInstance.CreatedUtc
                    }
                );

            context.For<WorkflowInstanceBlockingActivitiesIndex>()
                .Map(workflowInstance =>
                    workflowInstance.BlockingActivities.Select(x =>
                    new WorkflowInstanceBlockingActivitiesIndex
                    {
                        ActivityId = x.ActivityId,
                        ActivityName = x.Name,
                        ActivityIsStart = x.IsStart,
                        WorkflowDefinitionId = workflowInstance.WorkflowTypeId,
                        WorkflowInstanceId = workflowInstance.WorkflowId,
                        WorkflowInstanceCorrelationId = workflowInstance.CorrelationId ?? ""
                    })
                );
        }
    }
}
