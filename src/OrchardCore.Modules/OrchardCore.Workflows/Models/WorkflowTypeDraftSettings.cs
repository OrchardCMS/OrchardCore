namespace OrchardCore.Workflows.Models;

/// <summary>
/// The workflow type properties that can be edited in a <see cref="WorkflowTypeDraft"/>.
/// </summary>
public sealed class WorkflowTypeDraftSettings
{
    /// <summary>
    /// See <see cref="WorkflowType.Name"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.IsEnabled"/>.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.IsSingleton"/>.
    /// </summary>
    public bool IsSingleton { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.LockTimeout"/>.
    /// </summary>
    public int LockTimeout { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.LockExpiration"/>.
    /// </summary>
    public int LockExpiration { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.DeleteFinishedWorkflows"/>.
    /// </summary>
    public bool DeleteFinishedWorkflows { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.IsActivity"/>.
    /// </summary>
    public bool IsActivity { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.BranchingMode"/>.
    /// </summary>
    public WorkflowBranchingMode BranchingMode { get; set; }
}
