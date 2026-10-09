using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// A version of a workflow type, as listed by the designer.
/// </summary>
public sealed class WorkflowDesignerVersion
{
    /// <summary>
    /// The <see cref="WorkflowTypeVersion.VersionId"/>.
    /// </summary>
    public string VersionId { get; init; }

    /// <summary>
    /// The <see cref="WorkflowTypeVersion.Version"/> number.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// The name of the workflow type when the version was created.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// When the version was created.
    /// </summary>
    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// The name of the user who created the version, if any.
    /// </summary>
    public string CreatedBy { get; init; }

    /// <summary>
    /// Whether new instances start on this version.
    /// </summary>
    public bool IsPublished { get; init; }

    /// <summary>
    /// The number of instances that run on this version, or <see langword="null"/> when it isn't counted.
    /// </summary>
    public int? InstanceCount { get; init; }

    public static WorkflowDesignerVersion From(WorkflowTypeVersion version, WorkflowType workflowType, int? instanceCount = null)
        => version is null
            ? null
            : new()
            {
                VersionId = version.VersionId,
                Version = version.Version,
                Name = version.Name,
                CreatedUtc = version.CreatedUtc,
                CreatedBy = version.CreatedByUserName,
                IsPublished = version.VersionId == workflowType.VersionId,
                InstanceCount = instanceCount,
            };
}
