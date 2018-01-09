using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowManager
    {
        /// <summary>
        /// Creates a new <see cref="WorkflowContext"/>.
        /// </summary>
        WorkflowContext CreateWorkflowContext(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord, IDictionary<string, object> input = null);

        /// <summary>
        /// Creates a new <see cref="ActivityContext"/>.
        /// </summary>
        /// <param name="activityRecord"></param>
        ActivityContext CreateActivityContext(ActivityRecord activityRecord);

        /// <summary>
        /// Triggers a specific <see cref="IEvent"/>, and provides context if the event is
        /// actually executed.
        /// </summary>
        /// <param name="name">The type of the event to trigger, e.g. ContentPublishedEvent.</param>
        /// <param name="input">An object containing context for the event.</param>
        /// <param name="correlationId">Optionally specify a application-specific value to associate the workflow instance with. For example, a content item ID.</param>
        Task TriggerEventAsync(string name, IDictionary<string, object> input = null, string correlationId = null);

        /// <summary>
        /// Starts a new workflow using the specified workflow definition.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition to start.</param>
        /// <param name="input">Optionally specify any inputs to be used by the workflow.</param>
        /// <param name="correlationId">Optionally specify a application-specific value to associate the workflow instance with. For example, a content item ID.</param>
        /// <param name="startActivityName">If a workflow definition contains multiple start activities, you can specify which one to use. If none specified, the first one will be used.</param>
        /// <returns>Returns the created workflow context. Can be used for further inspection of the workflow state.</returns>
        Task<WorkflowContext> StartWorkflowAsync(WorkflowDefinitionRecord workflowDefinition, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null);

        /// <summary>
        /// Resumes the specified workflow instance at the specified activity.
        /// </summary>
        Task<WorkflowContext> ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance, AwaitingActivityRecord awaitingActivity, IDictionary<string, object> input = null);

        /// <summary>
        /// Resumes the specified workflow instance.
        /// </summary>
        Task<IList<WorkflowContext>> ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance, IDictionary<string, object> input = null);

        /// <summary>
        /// Executes the specified workflow starting at the specified activity.
        /// </summary>
        Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowContext workflowContext, ActivityRecord activity);
    }

    public static class WorkflowManagerExtensions
    {
        public static Task TriggerEventAsync(this IWorkflowManager workflowManager, string name, object input = null, string correlationId = null)
        {
            return workflowManager.TriggerEventAsync(name, new RouteValueDictionary(input));
        }

        public static Task<WorkflowContext> StartWorkflowAsync(this IWorkflowManager workflowManager, WorkflowDefinitionRecord workflowDefinition, object input = null, string startActivityName = null, string correlationId = null)
        {
            return workflowManager.StartWorkflowAsync(workflowDefinition, new RouteValueDictionary(input), startActivityName, correlationId);
        }
    }
}