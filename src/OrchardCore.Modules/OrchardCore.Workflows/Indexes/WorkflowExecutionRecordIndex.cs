using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes;

public sealed class WorkflowExecutionRecordIndex : MapIndex
{
    public string WorkflowId { get; set; }

    public int Sequence { get; set; }
}

public sealed class WorkflowExecutionRecordIndexProvider : IndexProvider<WorkflowExecutionRecord>
{
    public WorkflowExecutionRecordIndexProvider()
        => CollectionName = WorkflowExecutionRecord.Collection;

    public override void Describe(DescribeContext<WorkflowExecutionRecord> context)
        => context.For<WorkflowExecutionRecordIndex>()
            .Map(record => new WorkflowExecutionRecordIndex
            {
                WorkflowId = record.WorkflowId,
                Sequence = record.Sequence,
            });
}
