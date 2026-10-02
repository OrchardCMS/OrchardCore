using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Manages the <see cref="WorkflowTypeDraft"/> of each workflow type: the designer saves into the draft, and
/// <see cref="PublishAsync"/> applies it to the live <see cref="WorkflowType"/> through <see cref="IWorkflowTypeStore"/>.
/// </summary>
/// <remarks>
/// Every change takes the revision the caller last saw. When it doesn't match the current revision of the
/// draft (0 when there is no draft), the change is rejected with <see cref="WorkflowTypeDraftStatus.Conflict"/>.
/// The first change creates the draft as a copy of the live workflow type.
/// </remarks>
public interface IWorkflowTypeDraftManager
{
    /// <summary>
    /// Returns the draft of a workflow type, or <see langword="null"/> when it has none.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    Task<WorkflowTypeDraft> GetAsync(string workflowTypeId);

    /// <summary>
    /// Returns the draft of a workflow type, creating it as a deep copy of the live type when it has none.
    /// </summary>
    /// <param name="workflowType">The live workflow type.</param>
    Task<WorkflowTypeDraft> GetOrCreateAsync(WorkflowType workflowType);

    /// <summary>
    /// Saves positions, start flags, transitions, removed and restored activities into the draft. Removed
    /// activities are kept aside so they can be restored with their properties. Transitions whose source or
    /// destination activity doesn't exist are dropped.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    /// <param name="expectedRevision">The revision the caller last saw.</param>
    /// <param name="update">The canvas changes.</param>
    Task<WorkflowTypeDraftResult> SaveGraphAsync(string workflowTypeId, int expectedRevision, WorkflowGraphUpdate update);

    /// <summary>
    /// Adds an activity to the draft. The first event added to a workflow without a start activity becomes the
    /// start activity.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    /// <param name="expectedRevision">The revision the caller last saw.</param>
    /// <param name="activityName">The name of the activity type.</param>
    /// <param name="x">The left coordinate.</param>
    /// <param name="y">The top coordinate.</param>
    Task<WorkflowTypeDraftResult> AddActivityAsync(string workflowTypeId, int expectedRevision, string activityName, int x, int y);

    /// <summary>
    /// Replaces the properties of an activity of the draft, and removes the transitions of the outcomes the
    /// activity no longer produces.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    /// <param name="expectedRevision">The revision the caller last saw.</param>
    /// <param name="activityId">The <see cref="ActivityRecord.ActivityId"/>.</param>
    /// <param name="properties">The new properties of the activity.</param>
    Task<WorkflowTypeDraftResult> UpdateActivityAsync(string workflowTypeId, int expectedRevision, string activityId, JsonObject properties);

    /// <summary>
    /// Updates the workflow type properties of the draft.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    /// <param name="expectedRevision">The revision the caller last saw.</param>
    /// <param name="settings">The new properties.</param>
    Task<WorkflowTypeDraftResult> UpdateSettingsAsync(string workflowTypeId, int expectedRevision, WorkflowTypeDraftSettings settings);

    /// <summary>
    /// Applies the draft to the live workflow type through <see cref="IWorkflowTypeStore.SaveAsync"/>, then
    /// deletes the draft. A draft with <see cref="WorkflowDesignIssueSeverity.Error"/> issues isn't published.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    /// <param name="expectedRevision">The revision the caller last saw.</param>
    Task<WorkflowTypeDraftResult> PublishAsync(string workflowTypeId, int expectedRevision);

    /// <summary>
    /// Deletes the draft of a workflow type, if any, so the designer shows the live definition again.
    /// </summary>
    /// <param name="workflowTypeId">The <see cref="WorkflowType.WorkflowTypeId"/>.</param>
    Task DiscardAsync(string workflowTypeId);

    /// <summary>
    /// Returns the validation issues of a draft.
    /// </summary>
    /// <param name="draft">The draft to validate.</param>
    Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowTypeDraft draft);

    /// <summary>
    /// Returns the validation issues of a live workflow type.
    /// </summary>
    /// <param name="workflowType">The workflow type to validate.</param>
    Task<IReadOnlyList<WorkflowDesignIssue>> ValidateAsync(WorkflowType workflowType);
}
