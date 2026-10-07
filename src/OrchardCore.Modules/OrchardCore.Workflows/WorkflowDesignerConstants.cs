using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows;

/// <summary>
/// Constants used by the workflow designer.
/// </summary>
public static class WorkflowDesignerConstants
{
    /// <summary>
    /// The codes of the <see cref="WorkflowDesignIssue"/> rules.
    /// </summary>
    public static class IssueCodes
    {
        /// <summary>
        /// The workflow has activities but no start activity, so it never runs.
        /// </summary>
        public const string MissingStartActivity = nameof(MissingStartActivity);

        /// <summary>
        /// An activity's type is not registered, usually because its feature is disabled.
        /// </summary>
        public const string MissingActivity = nameof(MissingActivity);

        /// <summary>
        /// A transition points to a source or destination activity that doesn't exist.
        /// </summary>
        public const string InvalidTransition = nameof(InvalidTransition);

        /// <summary>
        /// An outcome has more than one outgoing transition; the engine only follows the first one.
        /// </summary>
        public const string DuplicateOutcomeTransition = nameof(DuplicateOutcomeTransition);

        /// <summary>
        /// An activity can't be reached from any start activity.
        /// </summary>
        public const string UnreachableActivity = nameof(UnreachableActivity);

        /// <summary>
        /// A Set Variable activity or an output binding names a variable the workflow doesn't declare.
        /// </summary>
        public const string UndeclaredVariable = nameof(UndeclaredVariable);

        /// <summary>
        /// An output is bound to a variable whose type its values may not convert to.
        /// </summary>
        public const string OutputTypeMismatch = nameof(OutputTypeMismatch);

        /// <summary>
        /// An Execute Workflow task doesn't say which workflow to run.
        /// </summary>
        public const string MissingWorkflowToExecute = nameof(MissingWorkflowToExecute);

        /// <summary>
        /// An Execute Workflow task runs the workflow it belongs to.
        /// </summary>
        public const string RecursiveWorkflowExecution = nameof(RecursiveWorkflowExecution);
    }
}
