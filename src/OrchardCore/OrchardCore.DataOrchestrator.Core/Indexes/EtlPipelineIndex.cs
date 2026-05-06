using OrchardCore.DataOrchestrator.Models;
using YesSql.Indexes;

namespace OrchardCore.DataOrchestrator.Indexes;

/// <summary>
/// YesSql index for querying ETL pipeline definitions.
/// </summary>
public sealed class EtlPipelineIndex : MapIndex
{
    public string PipelineId { get; set; }

    public string Name { get; set; }

    public bool IsEnabled { get; set; }
}

/// <summary>
/// YesSql index provider for <see cref="EtlPipelineDefinition"/>.
/// </summary>
public sealed class EtlPipelineIndexProvider : IndexProvider<EtlPipelineDefinition>
{
    public override void Describe(DescribeContext<EtlPipelineDefinition> context)
    {
        context.For<EtlPipelineIndex>()
            .Map(pipeline => new EtlPipelineIndex
            {
                PipelineId = pipeline.PipelineId,
                Name = pipeline.Name,
                IsEnabled = pipeline.IsEnabled,
            });
    }
}
