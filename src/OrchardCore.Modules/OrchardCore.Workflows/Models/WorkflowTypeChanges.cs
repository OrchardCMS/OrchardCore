namespace OrchardCore.Workflows.Models;

/// <summary>
/// The differences between two definitions of a workflow type, from an earlier one to a later one.
/// </summary>
public sealed class WorkflowTypeChanges
{
    /// <summary>
    /// The activities only the later definition has.
    /// </summary>
    public IReadOnlyList<string> AddedActivityIds { get; init; } = [];

    /// <summary>
    /// The activities only the earlier definition has.
    /// </summary>
    public IReadOnlyList<string> RemovedActivityIds { get; init; } = [];

    /// <summary>
    /// The activities whose type, start flag or properties changed.
    /// </summary>
    public IReadOnlyList<string> ChangedActivityIds { get; init; } = [];

    /// <summary>
    /// The activities that only moved on the canvas.
    /// </summary>
    public IReadOnlyList<string> MovedActivityIds { get; init; } = [];

    /// <summary>
    /// The transitions only the later definition has, as <see cref="WorkflowDesignIssue.GetTransitionKey"/> keys.
    /// </summary>
    public IReadOnlyList<string> AddedTransitionKeys { get; init; } = [];

    /// <summary>
    /// The transitions only the earlier definition has, as <see cref="WorkflowDesignIssue.GetTransitionKey"/> keys.
    /// </summary>
    public IReadOnlyList<string> RemovedTransitionKeys { get; init; } = [];

    /// <summary>
    /// The names of the <see cref="WorkflowType"/> properties that changed, among those a version holds and the name.
    /// </summary>
    public IReadOnlyList<string> ChangedSettings { get; init; } = [];

    /// <summary>
    /// Whether the definitions differ at all.
    /// </summary>
    public bool HasChanges
        => AddedActivityIds.Count > 0 ||
           RemovedActivityIds.Count > 0 ||
           ChangedActivityIds.Count > 0 ||
           MovedActivityIds.Count > 0 ||
           AddedTransitionKeys.Count > 0 ||
           RemovedTransitionKeys.Count > 0 ||
           ChangedSettings.Count > 0;
}
