using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

public static class WorkflowTypeVersionExtensions
{
    /// <summary>
    /// Returns a workflow type that runs <paramref name="version"/>: it has the identity, name and enabled state
    /// of <paramref name="workflowType"/>, and the activities, transitions and execution settings of the version.
    /// </summary>
    /// <remarks>
    /// The result is a transient object: never save it, as it would replace the current definition of the
    /// workflow type with the version.
    /// </remarks>
    /// <param name="version">The version to run.</param>
    /// <param name="workflowType">The workflow type the version belongs to.</param>
    public static WorkflowType ToWorkflowType(this WorkflowTypeVersion version, WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(workflowType);

        var result = new WorkflowType
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            VersionId = version.VersionId,
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            Properties = workflowType.Properties?.DeepClone().AsObject() ?? [],
            Activities = version.Activities.Select(activity => activity.Clone()).ToList(),
            Variables = version.Variables.Select(variable => variable.Clone()).ToList(),
            Transitions = version.Transitions.Select(transition => transition.Clone()).ToList(),
        };

        version.CopyExecutionSettingsTo(result);

        return result;
    }
}
