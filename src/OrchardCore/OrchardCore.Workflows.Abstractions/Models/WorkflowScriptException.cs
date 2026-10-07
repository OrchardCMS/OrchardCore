namespace OrchardCore.Workflows.Models;

/// <summary>
/// Faults a workflow instance at the activity whose scripts failed, when its workflow type faults on script errors
/// (<see cref="WorkflowType.FaultOnScriptErrors"/>).
/// </summary>
public sealed class WorkflowScriptException : Exception
{
    public WorkflowScriptException(string message)
        : base(message)
    {
    }
}
