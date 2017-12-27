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
        WorkflowContext CreateWorkflowContext(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord);

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
        Task TriggerEventAsync(string name, IDictionary<string, object> input);

        /// <summary>
        /// Starts a new workflow using the specified workflow definition.
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition to start.</param>
        /// <param name="input">Optional. Specify any inputs to be used by the workflow.</param>
        /// <param name="startActivityName">Optional. If a workflow definition contains multiple start activities, you can specify which one to use. If none specified, the first one will be used.</param>
        /// <returns>Returns the created workflow context. Can be used for further inspection of the workflow state.</returns>
        Task<WorkflowContext> StartWorkflowAsync(WorkflowDefinitionRecord workflowDefinition, IDictionary<string, object> input = null, string startActivityName = null);

        /// <summary>
        /// Executes the specified workflow starting at the specified activity.
        /// </summary>
        Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowContext workflowContext, ActivityRecord activity);
    }

    public static class WorkflowManagerExtensions
    {
        public static Task TriggerEventAsync(this IWorkflowManager workflowManager, string name, object input = null)
        {
            return workflowManager.TriggerEventAsync(name, new RouteValueDictionary(input));
        }

        public static Task<WorkflowContext> StartWorkflowAsync(this IWorkflowManager workflowManager, WorkflowDefinitionRecord workflowDefinition, object input = null, string startActivityName = null)
        {
            return workflowManager.StartWorkflowAsync(workflowDefinition, new RouteValueDictionary(input), startActivityName);
        }
    }
}