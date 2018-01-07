using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly IWorkflowInstanceRepository _workInstanceRepository;
        private readonly IScriptingManager _scriptingManager;
        private readonly IEnumerable<IWorkflowContextHandler> _workflowContextHandlers;
        private readonly ILogger<WorkflowManager> _logger;
        private readonly ILogger<WorkflowContext> _workflowContextLogger;
        private readonly ILogger<MissingActivity> _missingActivityLogger;
        private readonly IStringLocalizer<MissingActivity> _missingActivityLocalizer;

        public WorkflowManager
        (
            IActivityLibrary activityLibrary,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IScriptingManager scriptingManager,
            IEnumerable<IWorkflowContextHandler> workflowContextHandlers,
            ILogger<WorkflowManager> logger,
            ILogger<WorkflowContext> workflowContextLogger,
            ILogger<MissingActivity> missingActivityLogger,
            IStringLocalizer<MissingActivity> missingActivityLocalizer
        )
        {
            _activityLibrary = activityLibrary;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workInstanceRepository = workflowInstanceRepository;
            _scriptingManager = scriptingManager;
            _workflowContextHandlers = workflowContextHandlers;
            _logger = logger;
            _workflowContextLogger = workflowContextLogger;
            _missingActivityLogger = missingActivityLogger;
            _missingActivityLocalizer = missingActivityLocalizer;
        }

        public WorkflowContext CreateWorkflowContext(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord)
        {
            var activityQuery = workflowDefinitionRecord.Activities.Select(CreateActivityContext).Where(x => x != null);
            return new WorkflowContext(workflowDefinitionRecord, workflowInstanceRecord, activityQuery, _workflowContextHandlers, _scriptingManager, _workflowContextLogger);
        }

        public ActivityContext CreateActivityContext(ActivityRecord activityRecord)
        {
            var activity = _activityLibrary.InstantiateActivity<IActivity>(activityRecord.Name, activityRecord.Properties);

            if (activity == null)
            {
                _logger.LogWarning($"Requested activity '{activityRecord.Name}' does not exist in the library. This could indicate a changed name or a missing feature. Replacing it with MissingActivity.");
                activity = new MissingActivity(_missingActivityLocalizer, _missingActivityLogger, activityRecord);
            }

            return new ActivityContext
            {
                ActivityRecord = activityRecord,
                Activity = activity
            };
        }

        public async Task TriggerEventAsync(string name, IDictionary<string, object> input = null, string correlationId = null)
        {
            var activity = _activityLibrary.GetActivityByName(name);

            if (activity == null)
            {
                _logger.LogError("Activity {0} was not found", name);
                return;
            }

            // Look for workflow definitions with a corresponding starting activity.
            var workflowsToStart = await _workflowDefinitionRepository.GetWorkflowDefinitionsByStartActivityAsync(name);

            // And any running workflow paused on this kind of activity for the specified target.
            // When an activity is restarted, all the other ones of the same workflow are cancelled.
            var awaitingWorkflowInstances = await _workInstanceRepository.GetWaitingWorkflowInstancesAsync(name, correlationId);

            // If no activity record is matching the event, do nothing.
            if (!workflowsToStart.Any() && !awaitingWorkflowInstances.Any())
            {
                return;
            }

            // Resume pending workflows.
            foreach (var workflowInstance in awaitingWorkflowInstances)
            {
                // Merge additional input, if any.
                if (input?.Any() == true)
                {
                    var workflowState = workflowInstance.State.ToObject<WorkflowState>();
                    workflowState.Input.Merge(input);
                    workflowInstance.State = JObject.FromObject(workflowState);
                }

                await ResumeWorkflowAsync(workflowInstance);
            }

            // Start new workflows.
            foreach (var workflowToStart in workflowsToStart)
            {
                var startActivity = workflowToStart.Activities.FirstOrDefault(x => x.IsStart && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                if (startActivity != null)
                {
                    await StartWorkflowAsync(workflowToStart, startActivity, input);
                }
            }
        }

        public async Task<IList<WorkflowContext>> ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance)
        {
            var workflowContexts = new List<WorkflowContext>();
            foreach (var awaitingActivity in workflowInstance.AwaitingActivities.ToList())
            {
                var context = await ResumeWorkflowAsync(workflowInstance, awaitingActivity);
                workflowContexts.Add(context);
            }

            return workflowContexts;
        }

        public async Task<WorkflowContext> ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance, AwaitingActivityRecord awaitingActivity)
        {
            var workflowDefinition = await _workflowDefinitionRepository.GetWorkflowDefinitionAsync(workflowInstance.DefinitionId);
            var activityRecord = workflowDefinition.Activities.SingleOrDefault(x => x.Id == awaitingActivity.ActivityId);
            var workflowContext = CreateWorkflowContext(workflowDefinition, workflowInstance);

            workflowContext.Status = WorkflowStatus.Resuming;

            // Signal every activity that the workflow is about to be resumed.
            var cancellationToken = new CancellationToken();

            await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnWorkflowResumingAsync(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                return workflowContext;
            }

            // Signal every activity that the workflow is resumed.
            await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnWorkflowResumedAsync(workflowContext));

            // Remove the awaiting activity.
            workflowContext.WorkflowInstance.AwaitingActivities.Remove(awaitingActivity);

            // Resume the workflow at the specified blocking activity.
            var blockedOn = (await ExecuteWorkflowAsync(workflowContext, activityRecord)).ToList();

            // Check if the workflow halted on any blocking activities, and if there are no more awaiting activities.
            if (blockedOn.Count == 0 && workflowContext.WorkflowInstance.AwaitingActivities.Count == 0)
            {
                // No, delete the workflow.
                workflowContext.Status = WorkflowStatus.Finished;
                await _workInstanceRepository.DeleteAsync(workflowContext.WorkflowInstance);
            }
            else
            {
                // Add the new ones.
                workflowContext.Status = WorkflowStatus.Halted;
                foreach (var blocking in blockedOn)
                {
                    workflowContext.WorkflowInstance.AwaitingActivities.Add(AwaitingActivityRecord.FromActivity(blocking));
                }

                // Serialize state.
                await _workInstanceRepository.SaveAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<WorkflowContext> StartWorkflowAsync(WorkflowDefinitionRecord workflowDefinition, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null)
        {
            if (startActivity == null)
            {
                startActivity = workflowDefinition.Activities.FirstOrDefault(x => x.IsStart);

                if (startActivity == null)
                {
                    throw new InvalidOperationException($"Workflow with ID {workflowDefinition.Id} does not have a start activity.");
                }
            }

            // Create a new workflow instance.
            var workflowInstance = new WorkflowInstanceRecord
            {
                DefinitionId = workflowDefinition.Id,
                Uid = Guid.NewGuid().ToString("N"),
                State = JObject.FromObject(new WorkflowState { Input = input ?? new Dictionary<string, object>() }),
                CorrelationId = correlationId
            };

            // Create a workflow context.
            var workflowContext = CreateWorkflowContext(workflowDefinition, workflowInstance);
            workflowContext.Status = WorkflowStatus.Starting;

            // Signal every activity that the workflow is about to start.
            var cancellationToken = new CancellationToken();
            await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnWorkflowStartingAsync(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                return workflowContext;
            }

            // Signal every activity that the workflow has started.
            await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnWorkflowStartedAsync(workflowContext));

            // Execute the activity.
            var blockedOn = (await ExecuteWorkflowAsync(workflowContext, startActivity)).ToList();

            // Is the workflow halted on a blocking activity?
            if (blockedOn.Count == 0)
            {
                // No, nothing to do.
                workflowContext.Status = WorkflowStatus.Finished;
            }
            else
            {
                // Workflow halted, create a workflow state.
                workflowContext.Status = WorkflowStatus.Halted;
                foreach (var blocking in blockedOn)
                {
                    workflowContext.WorkflowInstance.AwaitingActivities.Add(AwaitingActivityRecord.FromActivity(blocking));
                }

                // Serialize state.
                await _workInstanceRepository.SaveAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowContext workflowContext, ActivityRecord activity)
        {
            var definition = await _workflowDefinitionRepository.GetWorkflowDefinitionAsync(workflowContext.WorkflowDefinition.Id);
            var firstPass = true;
            var scheduled = new Stack<ActivityRecord>();
            var blocking = new List<ActivityRecord>();

            workflowContext.Status = WorkflowStatus.Executing;
            scheduled.Push(activity);

            while (scheduled.Count > 0)
            {
                activity = scheduled.Pop();

                var activityContext = workflowContext.GetActivity(activity.Id);

                // While there is an activity to process.
                if (!firstPass)
                {
                    if (activityContext.Activity.IsEvent())
                    {
                        blocking.Add(activity);
                        continue;
                    }
                }
                else
                {
                    firstPass = false;
                }

                // Check if the current activity can execute.
                if (!await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
                {
                    // No, so break out and return.
                    break;
                }

                // Signal every activity that the activity is about to be executed.
                var cancellationToken = new CancellationToken();
                await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnActivityExecutingAsync(workflowContext, activityContext, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    // Activity is aborted.
                    continue;
                }

                // Execute the current activity.
                IList<string> outcomes;

                try
                {
                    outcomes = (await activityContext.Activity.ExecuteAsync(workflowContext, activityContext)).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while executing an activity. Workflow ID: {definition.Id}. Activity: {activityContext.ActivityRecord.Id}, {activityContext.ActivityRecord.Name}");
                    workflowContext.Fault(ex, activityContext);
                    return blocking.Distinct();
                }

                // Signal every activity that the activity is executed.
                await InvokeActivitiesAsync(workflowContext, async x => await x.Activity.OnActivityExecutedAsync(workflowContext, activityContext));

                foreach (var outcome in outcomes)
                {
                    // Look for next activity in the graph.
                    var transition = definition.Transitions.FirstOrDefault(x => x.SourceActivityId == activity.Id && x.SourceOutcomeName == outcome);

                    if (transition != null)
                    {
                        var destinationActivity = workflowContext.WorkflowDefinition.Activities.SingleOrDefault(x => x.Id == transition.DestinationActivityId);
                        scheduled.Push(destinationActivity);
                    }
                }
            }

            // Apply Distinct() as two paths could block on the same activity.
            return blocking.Distinct();
        }

        /// <summary>
        /// Executes a specific action on all the activities of a workflow.
        /// </summary>
        private async Task InvokeActivitiesAsync(WorkflowContext workflowContext, Func<ActivityContext, Task> action)
        {
            await workflowContext.Activities.InvokeAsync(x => action(x), _logger);
        }
    }
}
