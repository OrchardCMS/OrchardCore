using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowInstanceIndex : MapIndex
    {
        public int WorkflowDefinitionId { get; set; }
        public int WorkflowInstanceId { get; set; }
    }

    public class WorkflowInstanceByAwaitingActivitiesIndex : MapIndex
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public bool ActivityIsStart { get; set; }
        public int WorkflowInstanceId { get; set; }
        public string WorkflowInstanceUid { get; set; }
        public string WorkflowInstanceCorrelationId { get; set; }
    }

    public class WorkflowInstanceIndexProvider : IndexProvider<WorkflowInstanceRecord>
    {
        public override void Describe(DescribeContext<WorkflowInstanceRecord> context)
        {
            context.For<WorkflowInstanceIndex>()
                .Map(workflowInstance =>
                    new WorkflowInstanceIndex
                    {
                        WorkflowDefinitionId = workflowInstance.DefinitionId,
                        WorkflowInstanceId = workflowInstance.Id
                    }
                );

            context.For<WorkflowInstanceByAwaitingActivitiesIndex>()
                .Map(workflowInstance =>
                    workflowInstance.AwaitingActivities.Select(x =>
                    new WorkflowInstanceByAwaitingActivitiesIndex
                    {
                        ActivityId = x.ActivityId,
                        ActivityName = x.Name,
                        ActivityIsStart = x.IsStart,
                        WorkflowInstanceId = workflowInstance.Id,
                        WorkflowInstanceUid = workflowInstance.Uid,
                        WorkflowInstanceCorrelationId = workflowInstance.CorrelationId
                    })
                );
        }
    }
}
