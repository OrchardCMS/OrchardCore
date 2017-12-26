using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Entities;
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
        /// <param name="name">The type of the event to trigger, e.g. Publish.</param>
        /// <param name="target">The target entity the event is related to.</param>
        /// <param name="context">An object containing context for the event.</param>
        Task TriggerEvent(string name, IEntity target, Func<Dictionary<string, object>> context);
    }
}