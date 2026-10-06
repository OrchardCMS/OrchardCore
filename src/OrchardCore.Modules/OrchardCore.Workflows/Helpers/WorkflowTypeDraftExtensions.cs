using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

/// <summary>
/// Copies content between <see cref="WorkflowType"/> and <see cref="WorkflowTypeDraft"/>. Activity properties
/// are deep-cloned, because activities created from records share their <see cref="JsonObject"/> instances.
/// </summary>
internal static class WorkflowTypeDraftExtensions
{
    public static WorkflowTypeDraft CreateDraft(this WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        return new WorkflowTypeDraft
        {
            WorkflowTypeId = workflowType.WorkflowTypeId,
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            IsSingleton = workflowType.IsSingleton,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
            Activities = workflowType.Activities.Select(Clone).ToList(),
            Transitions = workflowType.Transitions.Select(Clone).ToList(),
            Variables = workflowType.Variables.Select(variable => variable.Clone()).ToList(),
        };
    }

    /// <summary>
    /// Copies the content of the draft into the live workflow type, keeping its identity.
    /// </summary>
    public static void ApplyTo(this WorkflowTypeDraft draft, WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(workflowType);

        workflowType.Name = draft.Name;
        workflowType.IsEnabled = draft.IsEnabled;
        workflowType.IsSingleton = draft.IsSingleton;
        workflowType.LockTimeout = draft.LockTimeout;
        workflowType.LockExpiration = draft.LockExpiration;
        workflowType.DeleteFinishedWorkflows = draft.DeleteFinishedWorkflows;
        workflowType.Activities = draft.Activities.Select(Clone).ToList();
        workflowType.Transitions = draft.Transitions.Select(Clone).ToList();
        workflowType.Variables = draft.Variables.Select(variable => variable.Clone()).ToList();
    }

    /// <summary>
    /// Returns a transient workflow type with the content of the draft, used to build execution contexts
    /// (for example to compute outcomes). It is never saved.
    /// </summary>
    public static WorkflowType ToTransientWorkflowType(this WorkflowTypeDraft draft, WorkflowType workflowType)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(workflowType);

        var transient = new WorkflowType
        {
            Id = workflowType.Id,
            WorkflowTypeId = workflowType.WorkflowTypeId,
        };

        draft.ApplyTo(transient);

        return transient;
    }

    public static ActivityRecord Clone(this ActivityRecord record)
        => new()
        {
            ActivityId = record.ActivityId,
            Name = record.Name,
            X = record.X,
            Y = record.Y,
            IsStart = record.IsStart,
            Properties = record.Properties?.DeepClone().AsObject() ?? [],
        };

    public static Transition Clone(this Transition transition)
        => new()
        {
            Id = transition.Id,
            SourceActivityId = transition.SourceActivityId,
            SourceOutcomeName = transition.SourceOutcomeName,
            DestinationActivityId = transition.DestinationActivityId,
        };
}
