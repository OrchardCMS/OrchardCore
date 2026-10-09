using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

public interface IWorkflowManager
{
    /// <summary>
    /// Creates a new workflow instance for the specified workflow definition. The instance is pinned to the
    /// version of the definition (<see cref="WorkflowType.VersionId"/>): it resumes on that version even after a
    /// new one is published.
    /// </summary>
    Workflow NewWorkflow(WorkflowType workflowType, string correlationId = null);

    /// <summary>
    /// Creates a new <see cref="WorkflowExecutionContext"/>.
    /// </summary>
    Task<WorkflowExecutionContext> CreateWorkflowExecutionContextAsync(WorkflowType workflowType, Workflow workflow, IDictionary<string, object> input = null);

    /// <summary>
    /// Creates a new <see cref="ActivityContext"/>.
    /// </summary>
    /// <param name="activityRecord"></param>
    /// <param name="properties"></param>
    Task<ActivityContext> CreateActivityExecutionContextAsync(ActivityRecord activityRecord, JsonObject properties);

    /// <summary>
    /// Triggers a specific <see cref="OrchardCore.Workflows.Activities.IEvent"/>.
    /// </summary>
    /// <param name="name">The type of the event to trigger, e.g. ContentPublishedEvent.</param>
    /// <param name="input">An object containing context for the event.</param>
    /// <param name="correlationId">Optionally specify a application-specific value to associate the workflow instance with. For example, a content item ID.</param>
    /// <param name="isExclusive">
    /// If true, a new workflow instance is not created if an existing one is already halted on a starting activity related to this event. False by default.
    /// </param>
    /// <param name="isAlwaysCorrelated">
    /// If true, to be correlated a workflow instance only needs to be halted on an event activity of the related type, regardless the 'correlationId'. False by default.
    /// </param>
    Task<IEnumerable<WorkflowExecutionContext>> TriggerEventAsync(string name, IDictionary<string, object> input = null, string correlationId = null, bool isExclusive = false, bool isAlwaysCorrelated = false);

    /// <summary>
    /// Starts a new workflow using the specified workflow definition.
    /// </summary>
    /// <param name="workflowType">The workflow definition to start.</param>
    /// <param name="startActivity">If a workflow definition contains multiple start activities, you can specify which one to use. If none specified, the first one will be used.</param>
    /// <param name="input">Optionally specify any inputs to be used by the workflow.</param>
    /// <param name="correlationId">Optionally specify an application-specific value to associate the workflow instance with. For example, a content item ID.</param>
    /// <returns>Returns the created workflow context. Can be used for further inspection of the workflow state.</returns>
    Task<WorkflowExecutionContext> StartWorkflowAsync(WorkflowType workflowType, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null);

    /// <summary>
    /// Starts an instance of a workflow as the child of an activity of another instance, for example the Execute
    /// Workflow task. The child starts on its <c>StartedByWorkflowEvent</c> start activity, else on its first start
    /// activity, and records its parent (<see cref="Workflow.ParentWorkflowId"/>). When the child finishes or faults
    /// in a later run, while the parent's activity waits on it, the parent's activity is resumed with a
    /// <see cref="ChildWorkflowResult"/> input.
    /// </summary>
    /// <param name="workflowType">The workflow to start.</param>
    /// <param name="parentContext">The context of the parent instance.</param>
    /// <param name="parentActivityId">The activity of the parent that starts the child.</param>
    /// <param name="input">The input values of the child, which set its input variables.</param>
    /// <returns>The context of the child's first run.</returns>
    /// <exception cref="InvalidOperationException">The workflow has no start activity, or workflows already run each
    /// other too many levels deep in this run.</exception>
    Task<WorkflowExecutionContext> StartChildWorkflowAsync(WorkflowType workflowType, WorkflowExecutionContext parentContext, string parentActivityId, IDictionary<string, object> input = null);

    /// <summary>
    /// Starts a new workflow using the specified workflow definition. Restarting an instance runs the definition
    /// it is given, usually the current one, not the version the restarted instance ran on.
    /// </summary>
    /// <param name="workflowType">The workflow definition to start.</param>
    /// <param name="input">Optionally specify any inputs to be used by the workflow.</param>
    /// <param name="correlationId">Optionally specify an application-specific value to associate the workflow instance with. For example, a content item ID.</param>
    /// <returns>Returns the created workflow context. Can be used for further inspection of the workflow state.</returns>
    Task<WorkflowExecutionContext> RestartWorkflowAsync(WorkflowType workflowType, IDictionary<string, object> input = null, string correlationId = null);

    /// <summary>
    /// Resumes the specified workflow instance at the specified activity, on the version of the definition the
    /// instance is pinned to (<see cref="Workflow.WorkflowTypeVersionId"/>), or on the current definition for
    /// instances that aren't pinned.
    /// </summary>
    Task<WorkflowExecutionContext> ResumeWorkflowAsync(Workflow workflow, BlockingActivity awaitingActivity, IDictionary<string, object> input = null);

    /// <summary>
    /// Runs a faulted workflow instance again from an activity of the definition it runs on, with its current
    /// state, for example after fixing what made the activity fail. The instance's lock is held while it runs.
    /// </summary>
    /// <param name="workflow">The faulted instance.</param>
    /// <param name="activityId">The <see cref="ActivityRecord.ActivityId"/> to run from.</param>
    /// <returns>The context of the run, or <see langword="null"/> when the instance's lock couldn't be acquired.</returns>
    /// <exception cref="InvalidOperationException">The instance isn't faulted, or its workflow type doesn't exist.</exception>
    /// <exception cref="ArgumentException">The definition doesn't have the activity.</exception>
    Task<WorkflowExecutionContext> RetryActivityAsync(Workflow workflow, string activityId);

    /// <summary>
    /// Runs the next attempt of a task that faulted and is retried by its <see cref="ActivityRetryPolicy"/>
    /// (<see cref="Workflow.PendingRetry"/>), when it's due, as <see cref="RetryActivityAsync"/> does but counting
    /// the attempts made before.
    /// </summary>
    /// <param name="workflow">The faulted instance.</param>
    /// <returns>The context of the run, or <see langword="null"/> when the instance has no due retry or its lock
    /// couldn't be acquired.</returns>
    Task<WorkflowExecutionContext> RunDueRetryAsync(Workflow workflow);

    /// <summary>
    /// Executes the specified workflow starting at the specified activity.
    /// </summary>
    Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowExecutionContext workflowExecutionContext, ActivityRecord activity);
}

public static class WorkflowManagerExtensions
{
    public static Task<IEnumerable<WorkflowExecutionContext>> TriggerEventAsync(this IWorkflowManager workflowManager, string name, object input = null, string correlationId = null)
    {
        return workflowManager.TriggerEventAsync(name, new RouteValueDictionary(input), correlationId);
    }

    public static Task<WorkflowExecutionContext> RestartWorkflowAsync(this IWorkflowManager workflowManager, Workflow workflow, WorkflowType workflowType, JsonSerializerOptions jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        ArgumentNullException.ThrowIfNull(workflowType);

        var state = workflow.State.ToObject<WorkflowState>(jsonSerializerOptions);

        return workflowManager.RestartWorkflowAsync(workflowType, state.Input, workflow.CorrelationId);
    }
}
