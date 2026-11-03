using OrchardCore.DataPipelines.Models;
using YesSql.Indexes;

namespace OrchardCore.DataPipelines.Indexes;

/// <summary>
/// Indexes the pipelines.
/// </summary>
public sealed class DataPipelineIndex : MapIndex
{
    public const int MaxIdLength = 26;
    public const int MaxNameLength = 255;

    public long DocumentId { get; set; }

    public string PipelineId { get; set; }

    public string Name { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsPublished { get; set; }

    public DateTime ModifiedUtc { get; set; }
}

/// <summary>
/// Indexes the published versions of the pipelines.
/// </summary>
public sealed class DataPipelineVersionIndex : MapIndex
{
    public long DocumentId { get; set; }

    public string VersionId { get; set; }

    public string PipelineId { get; set; }

    public int Number { get; set; }

    public DateTime PublishedUtc { get; set; }
}

/// <summary>
/// Indexes the runs of the pipelines.
/// </summary>
public sealed class DataPipelineRunIndex : MapIndex
{
    public const int MaxCorrelationIdLength = 255;

    public long DocumentId { get; set; }

    public string RunId { get; set; }

    public string PipelineId { get; set; }

    public string Status { get; set; }

    public string CorrelationId { get; set; }

    public DateTime QueuedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public DateTime? HeartbeatUtc { get; set; }
}

/// <summary>
/// Indexes the state documents of the pipelines.
/// </summary>
public sealed class DataPipelineStateIndex : MapIndex
{
    public long DocumentId { get; set; }

    public string PipelineId { get; set; }
}

public sealed class DataPipelineStateIndexProvider : IndexProvider<DataPipelineStateDocument>
{
    public override void Describe(DescribeContext<DataPipelineStateDocument> context)
    {
        context.For<DataPipelineStateIndex>()
            .Map(state => new DataPipelineStateIndex { PipelineId = state.PipelineId });
    }
}

public sealed class DataPipelineIndexProvider : IndexProvider<DataPipeline>
{
    public override void Describe(DescribeContext<DataPipeline> context)
    {
        context.For<DataPipelineIndex>()
            .Map(pipeline => new DataPipelineIndex
            {
                PipelineId = pipeline.PipelineId,
                Name = pipeline.Name?.Length > DataPipelineIndex.MaxNameLength ? pipeline.Name[..DataPipelineIndex.MaxNameLength] : pipeline.Name,
                IsEnabled = pipeline.IsEnabled,
                IsPublished = pipeline.Published is not null,
                ModifiedUtc = pipeline.ModifiedUtc,
            });
    }
}

public sealed class DataPipelineVersionIndexProvider : IndexProvider<DataPipelineVersion>
{
    public override void Describe(DescribeContext<DataPipelineVersion> context)
    {
        context.For<DataPipelineVersionIndex>()
            .Map(version => new DataPipelineVersionIndex
            {
                VersionId = version.VersionId,
                PipelineId = version.PipelineId,
                Number = version.Number,
                PublishedUtc = version.PublishedUtc,
            });
    }
}

public sealed class DataPipelineRunIndexProvider : IndexProvider<DataPipelineRun>
{
    public override void Describe(DescribeContext<DataPipelineRun> context)
    {
        context.For<DataPipelineRunIndex>()
            .Map(run => new DataPipelineRunIndex
            {
                RunId = run.RunId,
                PipelineId = run.PipelineId,
                Status = run.Status.ToString(),
                CorrelationId = run.CorrelationId?.Length > DataPipelineRunIndex.MaxCorrelationIdLength ? run.CorrelationId[..DataPipelineRunIndex.MaxCorrelationIdLength] : run.CorrelationId,
                QueuedUtc = run.QueuedUtc,
                CompletedUtc = run.CompletedUtc,
                HeartbeatUtc = run.HeartbeatUtc,
            });
    }
}
