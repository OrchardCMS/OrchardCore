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

        return new WorkflowType
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
            VersionId = version.VersionId,
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            Properties = workflowType.Properties?.DeepClone().AsObject() ?? [],
            IsSingleton = version.IsSingleton,
            IsSingletonPerCorrelation = version.IsSingletonPerCorrelation,
            LockTimeout = version.LockTimeout,
            LockExpiration = version.LockExpiration,
            DeleteFinishedWorkflows = version.DeleteFinishedWorkflows,
            IsActivity = version.IsActivity,
            BranchingMode = version.BranchingMode,
            FaultOnScriptErrors = version.FaultOnScriptErrors,
            RecordActivityData = version.RecordActivityData,
            Activities = version.Activities
                .Select(activity => new ActivityRecord
                {
                    ActivityId = activity.ActivityId,
                    Name = activity.Name,
                    X = activity.X,
                    Y = activity.Y,
                    IsStart = activity.IsStart,
                    Properties = activity.Properties?.DeepClone().AsObject() ?? [],
                })
                .ToList(),
            Variables = version.Variables.Select(variable => variable.Clone()).ToList(),
            Transitions = version.Transitions
                .Select(transition => new Transition
                {
                    Id = transition.Id,
                    SourceActivityId = transition.SourceActivityId,
                    SourceOutcomeName = transition.SourceOutcomeName,
                    DestinationActivityId = transition.DestinationActivityId,
                })
                .ToList(),
        };
    }
}
