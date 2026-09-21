using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

public static class WorkflowTypeExtensions
{
    /// <summary>
    /// Returns <c>true</c> when one of the activities of the workflow type is a startup task.
    /// </summary>
    public static bool HasStartActivity(this WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        return workflowType.Activities.Any(activity => activity.IsStart);
    }

    /// <summary>
    /// Returns <c>true</c> when the workflow type has activities but none of them is a startup task,
    /// which means the workflow can never be triggered.
    /// </summary>
    public static bool IsMissingStartActivity(this WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        return workflowType.Activities.Count > 0 && !workflowType.HasStartActivity();
    }
}
