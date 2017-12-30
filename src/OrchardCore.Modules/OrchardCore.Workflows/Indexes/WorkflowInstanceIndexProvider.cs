using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowInstanceByAwaitingActivitiesIndex : MapIndex
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public bool ActivityIsStart { get; set; }
        public int WorkflowInstanceId { get; set; }
        public string WorkflowInstanceCorrelationId { get; set; }
    }

    public class WorkflowInstanceIndexProvider : IndexProvider<WorkflowInstanceRecord>
    {
        public override void Describe(DescribeContext<WorkflowInstanceRecord> context)
        {
            context.For<WorkflowInstanceByAwaitingActivitiesIndex>()
                .Map(workflowInstance =>
                    workflowInstance.AwaitingActivities.Select(x =>
                    new WorkflowInstanceByAwaitingActivitiesIndex
                    {
                        ActivityId = x.ActivityId,
                        ActivityName = x.Name,
                        ActivityIsStart = x.IsStart,
                        WorkflowInstanceId = workflowInstance.Id,
                        WorkflowInstanceCorrelationId = workflowInstance.CorrelationId
                    })
                );
        }
    }
}
