using OrchardCore.DataOrchestrator.Models;
using YesSql.Indexes;

namespace OrchardCore.DataOrchestrator.Indexes;

/// <summary>
/// YesSql index for querying ETL execution logs.
/// </summary>
public sealed class EtlExecutionLogIndex : MapIndex
{
    public string PipelineId { get; set; }

    public DateTime StartedUtc { get; set; }

    public string Status { get; set; }
}

/// <summary>
/// YesSql index provider for <see cref="EtlExecutionLog"/>.
/// </summary>
public sealed class EtlExecutionLogIndexProvider : IndexProvider<EtlExecutionLog>
{
    public override void Describe(DescribeContext<EtlExecutionLog> context)
    {
        context.For<EtlExecutionLogIndex>()
            .Map(log => new EtlExecutionLogIndex
            {
                PipelineId = log.PipelineId,
                StartedUtc = log.StartedUtc,
                Status = log.Status,
            });
    }
}
