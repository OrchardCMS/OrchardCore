using OrchardCore.Workflows.Models;
using YesSql.Indexes;

namespace OrchardCore.Workflows.Indexes;

/// <summary>
/// Indexes <see cref="WorkflowTypeVersion"/> documents.
/// </summary>
public sealed class WorkflowTypeVersionIndex : MapIndex
{
    /// <summary>
    /// The <see cref="WorkflowTypeVersion.WorkflowTypeId"/>.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// The <see cref="WorkflowTypeVersion.VersionId"/>.
    /// </summary>
    public string VersionId { get; set; }

    /// <summary>
    /// The <see cref="WorkflowTypeVersion.Version"/>.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The <see cref="WorkflowTypeVersion.CreatedUtc"/>.
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}

/// <summary>
/// Maps <see cref="WorkflowTypeVersion"/> documents to <see cref="WorkflowTypeVersionIndex"/>.
/// </summary>
public sealed class WorkflowTypeVersionIndexProvider : IndexProvider<WorkflowTypeVersion>
{
    public override void Describe(DescribeContext<WorkflowTypeVersion> context)
    {
        context.For<WorkflowTypeVersionIndex>()
            .Map(version => new WorkflowTypeVersionIndex
            {
                WorkflowTypeId = version.WorkflowTypeId,
                VersionId = version.VersionId,
                Version = version.Version,
                CreatedUtc = version.CreatedUtc,
            });
    }
}
