using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Scripting;
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
        private readonly ILogger _logger;

        public WorkflowManager
        (
            IActivityLibrary activityLibrary,
            IWorkflowDefinitionRepository workflowDefinitionRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            IScriptingManager scriptingManager,
            ILogger<WorkflowManager> logger
        )
        {
            _activityLibrary = activityLibrary;
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workInstanceRepository = workflowInstanceRepository;
            _scriptingManager = scriptingManager;
            _logger = logger;
        }

        public WorkflowContext CreateWorkflowContext(WorkflowDefinitionRecord workflowDefinitionRecord, WorkflowInstanceRecord workflowInstanceRecord)
        {
            var activityQuery = workflowDefinitionRecord.Activities.Select(CreateActivityContext);
            return new WorkflowContextImpl(workflowDefinitionRecord, workflowInstanceRecord, activityQuery, _scriptingManager);
        }

        public ActivityContext CreateActivityContext(ActivityRecord activityRecord)
        {
            var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);
            var entity = activity as Entity;

            entity.Properties = activityRecord.Properties;
            return new ActivityContext
            {
                ActivityRecord = activityRecord,
                Activity = activity
            };
        }

        public async Task TriggerEventAsync(string name, Func<IDictionary<string, object>> inputProvider)
        {
            var input = inputProvider();
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
            var awaitingWorkflowInstances = await _workInstanceRepository.GetPendingWorkflowInstancesByActivityAsync(name);

            // If no activity record is matching the event, do nothing.
            if (!workflowsToStart.Any() && !awaitingWorkflowInstances.Any())
            {
                return;
            }

            // Resume pending workflows.
            foreach (var workflowInstance in awaitingWorkflowInstances)
            {
                await ResumeWorkflowAsync(workflowInstance);
            }

            // Start new workflows.
            foreach (var workflowToStart in workflowsToStart)
            {
                await StartWorkflowAsync(workflowToStart, input, name);
            }
        }

        public async Task ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance)
        {
            foreach (var awaitingActivity in workflowInstance.AwaitingActivities.ToList())
            {
                await ResumeWorkflowAsync(workflowInstance, awaitingActivity);
            }
        }

        public async Task ResumeWorkflowAsync(WorkflowInstanceRecord workflowInstance, AwaitingActivityRecord awaitingActivity)
        {
            var workflowDefinition = await _workflowDefinitionRepository.GetWorkflowDefinitionAsync(workflowInstance.DefinitionId);
            var activityRecord = workflowDefinition.Activities.SingleOrDefault(x => x.Id == awaitingActivity.ActivityId);
            var workflowContext = CreateWorkflowContext(workflowDefinition, workflowInstance);

            // Signal every activity that the workflow is about to be resumed.
            var cancellationToken = new CancellationToken();
            InvokeActivities(workflowContext, x => x.Activity.OnWorkflowResuming(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                return;
            }

            // Signal every activity that the workflow is resumed.
            InvokeActivities(workflowContext, x => x.Activity.OnWorkflowResumed(workflowContext));

            // Remove the awaiting activity.
            workflowContext.WorkflowInstance.AwaitingActivities.Remove(awaitingActivity);

            // Resume the workflow at the specified blocking activity.
            var blockedOn = (await ExecuteWorkflowAsync(workflowContext, activityRecord)).ToList();

            // Check if the workflow halted on any blocking activities, and if there are no more awaiting activities.
            if (blockedOn.Count == 0 && workflowContext.WorkflowInstance.AwaitingActivities.Count == 0)
            {
                // No, delete the workflow.
                _workInstanceRepository.Delete(workflowContext.WorkflowInstance);
            }
            else
            {
                // Add the new ones.
                foreach (var blocking in blockedOn)
                {
                    workflowContext.WorkflowInstance.AwaitingActivities.Add(AwaitingActivityRecord.FromActivity(blocking));
                }

                // Serialize state.
                _workInstanceRepository.Save(workflowContext);
            }
        }

        public async Task<WorkflowContext> StartWorkflowAsync(WorkflowDefinitionRecord workflowDefinition, IDictionary<string, object> input = null, string startActivityName = null)
        {
            // Get the starting activity.
            var startActivityQuery = workflowDefinition.Activities.Where(x => x.IsStart);

            if (!string.IsNullOrWhiteSpace(startActivityName))
            {
                startActivityQuery = startActivityQuery.Where(x => x.Name == startActivityName);
            }

            var startActivity = startActivityQuery.First();

            // Create a new workflow instance.
            var workflowInstance = new WorkflowInstanceRecord
            {
                DefinitionId = workflowDefinition.Id,
                State = JObject.FromObject(new WorkflowState { Input = input ?? new Dictionary<string, object>() })
            };

            // Create a workflow context.
            var workflowContext = CreateWorkflowContext(workflowDefinition, workflowInstance);

            // Signal every activity that the workflow is about to start.
            var cancellationToken = new CancellationToken();
            InvokeActivities(workflowContext, x => x.Activity.OnWorkflowStarting(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                return workflowContext;
            }

            // Signal every activity that the workflow has started.
            InvokeActivities(workflowContext, x => x.Activity.OnWorkflowStarted(workflowContext));

            // Execute the activity.
            var blockedOn = (await ExecuteWorkflowAsync(workflowContext, startActivity)).ToList();

            // Is the workflow halted on a blocking activity?
            if (blockedOn.Count == 0)
            {
                // No, nothing to do.
            }
            else
            {
                // Workflow halted, create a workflow state.
                foreach (var blocking in blockedOn)
                {
                    workflowContext.WorkflowInstance.AwaitingActivities.Add(AwaitingActivityRecord.FromActivity(blocking));
                }

                // Serialize state.
                _workInstanceRepository.Save(workflowContext);
            }

            return workflowContext;
        }

        public async Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowContext workflowContext, ActivityRecord activity)
        {
            var definition = await _workflowDefinitionRepository.GetWorkflowDefinitionAsync(workflowContext.WorkflowDefinition.Id);
            var firstPass = true;
            var scheduled = new Stack<ActivityRecord>();

            scheduled.Push(activity);

            var blocking = new List<ActivityRecord>();

            while (scheduled.Count > 0)
            {
                activity = scheduled.Pop();

                var activityContext = workflowContext.GetActivity(activity.Id);

                // Check if the current activity can execute.
                if (!await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
                {
                    // No, so break out and return.
                    break;
                }

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

                // Signal every activity that the activity is about to be executed.
                var cancellationToken = new CancellationToken();
                InvokeActivities(workflowContext, x => x.Activity.OnActivityExecuting(workflowContext, activityContext, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    // Activity is aborted.
                    continue;
                }

                // Execute the current activity.
                var outcomes = (await activityContext.Activity.ExecuteAsync(workflowContext, activityContext)).ToList();

                // Signal every activity that the activity is executed.
                InvokeActivities(workflowContext, x => x.Activity.OnActivityExecuted(workflowContext, activityContext));

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
        private void InvokeActivities(WorkflowContext workflowContext, Action<ActivityContext> action)
        {
            foreach (var activity in workflowContext.Activities)
            {
                action(activity);
            }
        }
    }
}
