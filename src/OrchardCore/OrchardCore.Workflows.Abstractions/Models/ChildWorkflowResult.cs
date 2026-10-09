namespace OrchardCore.Workflows.Models;

/// <summary>
/// How a child instance (see <see cref="Services.IWorkflowManager.StartChildWorkflowAsync"/>) ended, passed to its
/// parent's waiting activity under the <see cref="InputKey"/> input when the child ends in a later run.
/// </summary>
public sealed class ChildWorkflowResult
{
    /// <summary>
    /// The input key of the result.
    /// </summary>
    public const string InputKey = "OrchardCore.Workflows.ChildWorkflowResult";

    /// <summary>
    /// The <see cref="Workflow.WorkflowId"/> of the child.
    /// </summary>
    public string WorkflowId { get; init; }

    /// <summary>
    /// <see cref="WorkflowStatus.Finished"/> or <see cref="WorkflowStatus.Faulted"/>.
    /// </summary>
    public WorkflowStatus Status { get; init; }

    /// <summary>
    /// The values of the child's output variables (<see cref="WorkflowVariableDefinition.IsOutput"/>).
    /// </summary>
    public IDictionary<string, object> Outputs { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Why the child faulted, if it did.
    /// </summary>
    public string FaultMessage { get; init; }

    /// <summary>
    /// The result of a child that ended.
    /// </summary>
    public static ChildWorkflowResult From(WorkflowExecutionContext childContext)
    {
        ArgumentNullException.ThrowIfNull(childContext);

        return new ChildWorkflowResult
        {
            WorkflowId = childContext.WorkflowId,
            Status = childContext.Status,
            Outputs = childContext.Variables.GetOutputs(),
            FaultMessage = childContext.Workflow.FaultMessage,
        };
    }
}
