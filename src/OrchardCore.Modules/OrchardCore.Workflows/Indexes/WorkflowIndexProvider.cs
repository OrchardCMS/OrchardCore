using System;
using System.Linq;
using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes
{
    public class WorkflowIndex : MapIndex
    {
        public int DocumentId { get; set; }
        public string WorkflowTypeId { get; set; }
        public string WorkflowId { get; set; }
        public int WorkflowStatus { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class WorkflowBlockingActivitiesIndex : MapIndex
    {
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public bool ActivityIsStart { get; set; }
        public string WorkflowTypeId { get; set; }
        public string WorkflowId { get; set; }
        public string WorkflowCorrelationId { get; set; }
    }

    public class WorkflowsByTypeIndex : ReduceIndex
    {
        public string WorkflowTypeId { get; set; }

        public int Count { get; set; }
    }

    public class WorkflowIndexProvider : IndexProvider<Workflow>
    {
        public override void Describe(DescribeContext<Workflow> context)
        {
            context.For<WorkflowIndex>()
                .Map(workflow =>
                    new WorkflowIndex
                    {
                        WorkflowTypeId = workflow.WorkflowTypeId,
                        WorkflowId = workflow.WorkflowId,
                        CreatedUtc = workflow.CreatedUtc,
                        WorkflowStatus = (int)workflow.Status
                    }
                );

            context.For<WorkflowBlockingActivitiesIndex>()
                .Map(workflow =>
                    workflow.BlockingActivities.Select(x =>
                    new WorkflowBlockingActivitiesIndex
                    {
                        ActivityId = x.ActivityId,
                        ActivityName = x.Name,
                        ActivityIsStart = x.IsStart,
                        WorkflowTypeId = workflow.WorkflowTypeId,
                        WorkflowId = workflow.WorkflowId,
                        WorkflowCorrelationId = workflow.CorrelationId ?? ""
                    })
                );

            context.For<WorkflowsByTypeIndex, string>()
                .Map(workflow => new WorkflowsByTypeIndex
                {
                    WorkflowTypeId = workflow.WorkflowTypeId,
                    Count = 1,
                })
                .Group(index => index.WorkflowTypeId)
                .Reduce(group => new WorkflowsByTypeIndex
                {
                    WorkflowTypeId = group.Key,
                    Count = group.Sum(type => type.Count),
                })
                .Delete((index, map) =>
                {
                    index.Count -= map.Sum(x => x.Count);
                    return index.Count > 0 ? index : null;
                });
        }
    }
}
