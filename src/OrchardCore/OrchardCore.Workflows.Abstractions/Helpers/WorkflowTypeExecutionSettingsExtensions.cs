using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

public static class WorkflowTypeExecutionSettingsExtensions
{
    /// <summary>
    /// Copies the execution settings of <paramref name="source"/> to <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The settings to copy.</param>
    /// <param name="target">The settings to set.</param>
    public static void CopyExecutionSettingsTo(this IWorkflowTypeExecutionSettings source, IWorkflowTypeExecutionSettings target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.IsSingleton = source.IsSingleton;
        target.IsSingletonPerCorrelation = source.IsSingletonPerCorrelation;
        target.LockTimeout = source.LockTimeout;
        target.LockExpiration = source.LockExpiration;
        target.DeleteFinishedWorkflows = source.DeleteFinishedWorkflows;
        target.IsActivity = source.IsActivity;
        target.BranchingMode = source.BranchingMode;
        target.FaultOnScriptErrors = source.FaultOnScriptErrors;
        target.RecordActivityData = source.RecordActivityData;
    }
}
