namespace OrchardCore.Workflows.Models;

/// <summary>
/// The settings of a <see cref="WorkflowType"/> that affect how its instances run, shared by the workflow type,
/// its versions, its draft and the forms that edit them.
/// </summary>
public interface IWorkflowTypeExecutionSettings
{
    /// <summary>
    /// See <see cref="WorkflowType.IsSingleton"/>.
    /// </summary>
    bool IsSingleton { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.IsSingletonPerCorrelation"/>.
    /// </summary>
    bool IsSingletonPerCorrelation { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.LockTimeout"/>.
    /// </summary>
    int LockTimeout { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.LockExpiration"/>.
    /// </summary>
    int LockExpiration { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.DeleteFinishedWorkflows"/>.
    /// </summary>
    bool DeleteFinishedWorkflows { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.IsActivity"/>.
    /// </summary>
    bool IsActivity { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.BranchingMode"/>.
    /// </summary>
    WorkflowBranchingMode BranchingMode { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.FaultOnScriptErrors"/>.
    /// </summary>
    bool FaultOnScriptErrors { get; set; }

    /// <summary>
    /// See <see cref="WorkflowType.RecordActivityData"/>.
    /// </summary>
    bool RecordActivityData { get; set; }
}
