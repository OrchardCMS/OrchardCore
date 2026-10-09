using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Stores the <see cref="WorkflowTypeVersion"/> snapshots of workflow types.
/// </summary>
/// <remarks>
/// <see cref="IWorkflowTypeStore.SaveAsync"/> creates the versions: callers don't need to, unless they write
/// workflow types without the store.
/// </remarks>
public interface IWorkflowTypeVersionStore
{
    /// <summary>
    /// Returns a version, or <see langword="null"/> when it doesn't exist.
    /// </summary>
    /// <param name="versionId">The <see cref="WorkflowTypeVersion.VersionId"/>.</param>
    Task<WorkflowTypeVersion> GetAsync(string versionId);

    /// <summary>
    /// Returns the versions of a workflow type, the most recent first.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    Task<IEnumerable<WorkflowTypeVersion>> ListAsync(string workflowTypeId);

    /// <summary>
    /// Returns the most recent version of a workflow type, or <see langword="null"/> when it has none.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    Task<WorkflowTypeVersion> GetLatestAsync(string workflowTypeId);

    /// <summary>
    /// Creates the next version of a workflow type when its activities, transitions or execution settings differ
    /// from its most recent version (or when it has none), and sets <see cref="WorkflowType.VersionId"/>.
    /// </summary>
    /// <param name="workflowType">The workflow type, before it is saved.</param>
    /// <returns>The version the workflow type now refers to.</returns>
    Task<WorkflowTypeVersion> CreateIfChangedAsync(WorkflowType workflowType);

    /// <summary>
    /// Returns the definition an instance runs: <paramref name="workflowType"/> itself when
    /// <paramref name="versionId"/> is empty or is its current version, otherwise that version of it (see
    /// <see cref="Helpers.WorkflowTypeVersionExtensions.ToWorkflowType"/>). A version that doesn't exist falls back
    /// to <paramref name="workflowType"/>.
    /// </summary>
    /// <param name="workflowType">The workflow type.</param>
    /// <param name="versionId">The <see cref="Workflow.WorkflowTypeVersionId"/> of the instance.</param>
    Task<WorkflowType> GetWorkflowTypeAsync(WorkflowType workflowType, string versionId);

    /// <summary>
    /// Deletes every version of a workflow type.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    Task DeleteAsync(string workflowTypeId);
}
