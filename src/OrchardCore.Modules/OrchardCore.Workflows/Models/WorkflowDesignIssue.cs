namespace OrchardCore.Workflows.Models;

/// <summary>
/// A problem found in a workflow type or draft by the designer validation.
/// </summary>
public sealed class WorkflowDesignIssue
{
    /// <summary>
    /// How serious the issue is.
    /// </summary>
    public WorkflowDesignIssueSeverity Severity { get; init; }

    /// <summary>
    /// A stable identifier of the rule that raised the issue, one of <see cref="WorkflowDesignerConstants.IssueCodes"/>.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// A localized description of the issue.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// The activity the issue is about, if any.
    /// </summary>
    public string ActivityId { get; init; }

    /// <summary>
    /// The transition the issue is about, if any, as returned by <see cref="GetTransitionKey(Transition)"/>.
    /// </summary>
    public string TransitionKey { get; init; }

    /// <summary>
    /// Returns the key that identifies a transition in issues:
    /// <c>{SourceActivityId}:{SourceOutcomeName}:{DestinationActivityId}</c>.
    /// </summary>
    public static string GetTransitionKey(Transition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        return $"{transition.SourceActivityId}:{transition.SourceOutcomeName}:{transition.DestinationActivityId}";
    }
}

/// <summary>
/// The severity of a <see cref="WorkflowDesignIssue"/>.
/// </summary>
public enum WorkflowDesignIssueSeverity
{
    /// <summary>
    /// The workflow can't be published until the issue is fixed.
    /// </summary>
    Error,

    /// <summary>
    /// The workflow can be published, but probably doesn't behave as intended.
    /// </summary>
    Warning,
}
