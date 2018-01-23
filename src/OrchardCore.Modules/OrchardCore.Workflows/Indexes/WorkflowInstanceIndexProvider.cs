using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowInstanceIndex : MapIndex
    {
        public string WorkflowDefinitionUid { get; set; }
        public string WorkflowInstanceUid { get; set; }
    }

    public class WorkflowInstanceByAwaitingActivitiesIndex : MapIndex
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public bool ActivityIsStart { get; set; }
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
                        WorkflowDefinitionUid = workflowInstance.WorkflowDefinition.Uid,
                        WorkflowInstanceUid = workflowInstance.Uid
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
                        WorkflowInstanceUid = workflowInstance.Uid,
                        WorkflowInstanceCorrelationId = workflowInstance.CorrelationId ?? "" // TODO: Can't compare NULL == NULL for some reason, so converting to empty string. See also https://github.com/sebastienros/yessql/issues/89
                    })
                );
        }
    }
}
