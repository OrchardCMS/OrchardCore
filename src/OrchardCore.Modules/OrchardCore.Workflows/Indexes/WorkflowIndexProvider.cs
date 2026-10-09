using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes;

public class WorkflowIndex : MapIndex
{
    public long DocumentId { get; set; }
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// The <see cref="Workflow.WorkflowTypeVersionId"/> of the instance, if any.
    /// </summary>
    public string WorkflowTypeVersionId { get; set; }

    public string WorkflowId { get; set; }
    public WorkflowStatus WorkflowStatus { get; set; }
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// When the next attempt of the task a faulted instance is retried from is due
    /// (<see cref="Workflow.PendingRetry"/>), if any.
    /// </summary>
    public DateTime? RetryDueUtc { get; set; }
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

public class WorkflowIndexProvider : IndexProvider<Workflow>
{
    public override void Describe(DescribeContext<Workflow> context)
    {
        context.For<WorkflowIndex>()
            .Map(workflow =>
                new WorkflowIndex
                {
                    WorkflowTypeId = workflow.WorkflowTypeId,
                    WorkflowTypeVersionId = workflow.WorkflowTypeVersionId,
                    WorkflowId = workflow.WorkflowId,
                    CreatedUtc = workflow.CreatedUtc,
                    WorkflowStatus = workflow.Status,
                    RetryDueUtc = workflow.Status == WorkflowStatus.Faulted ? workflow.PendingRetry?.DueUtc : null,
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
                    WorkflowCorrelationId = workflow.CorrelationId ?? "",
                })
            );
    }
}
