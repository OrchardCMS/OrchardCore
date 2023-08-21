using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public interface IActivity : IEntity
    {
        string Name { get; }
        LocalizedString DisplayText { get; }
        LocalizedString Category { get; }
        new JObject Properties { get; set; }
        bool HasEditor { get; }

        /// <summary>
        /// List of possible outcomes when the activity is executed.
        /// </summary>
        IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext);

        /// <summary>
        /// Whether the activity can transition to the next outcome. Can prevent the activity from being transitioned
        /// because a condition is not valid.
        /// </summary>
        Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);

        /// <summary>
        /// Executes the specified activity.
        /// </summary>
        Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);

        /// <summary>
        /// Resumes the specified activity.
        /// </summary>
        Task<ActivityExecutionResult> ResumeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);

        /// <summary>
        /// Executes before a workflow starts or resumes, giving activities an opportunity to read and store any values of interest.
        /// </summary>
        Task OnInputReceivedAsync(WorkflowExecutionContext workflowContext, IDictionary<string, object> input);

        /// <summary>
        /// Called on each activity when a workflow is about to start.
        /// </summary>
        Task OnWorkflowStartingAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called on each activity when a workflow has started.
        /// </summary>
        Task OnWorkflowStartedAsync(WorkflowExecutionContext context);

        /// <summary>
        /// Called on each activity when a workflow is about to be resumed.
        /// </summary>
        Task OnWorkflowResumingAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called on each activity when a workflow is resumed.
        /// </summary>
        Task OnWorkflowResumedAsync(WorkflowExecutionContext context);

        /// <summary>
        /// Called on each activity when an activity is about to be executed.
        /// </summary>
        Task OnActivityExecutingAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Called on each activity when an activity has been executed.
        /// </summary>
        Task OnActivityExecutedAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext);
    }
}
