using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowInstanceIdGenerator _workflowInstanceIdGenerator;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly Resolver<IEnumerable<IWorkflowExecutionContextHandler>> _workflowContextHandlers;
        private readonly Resolver<IEnumerable<IWorkflowValueSerializer>> _workflowValueSerializers;
        private readonly ILogger<WorkflowManager> _logger;
        private readonly ILogger<WorkflowExecutionContext> _workflowContextLogger;
        private readonly ILogger<MissingActivity> _missingActivityLogger;
        private readonly IStringLocalizer<MissingActivity> _missingActivityLocalizer;
        private readonly IClock _clock;

        public WorkflowManager
        (
            IActivityLibrary activityLibrary,
            IWorkflowDefinitionStore workflowDefinitionRepository,
            IWorkflowInstanceStore workflowInstanceRepository,
            IWorkflowInstanceIdGenerator workflowInstanceIdGenerator,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            Resolver<IEnumerable<IWorkflowExecutionContextHandler>> workflowContextHandlers,
            Resolver<IEnumerable<IWorkflowValueSerializer>> workflowValueSerializers,
            ILogger<WorkflowManager> logger,
            ILogger<WorkflowExecutionContext> workflowContextLogger,
            ILogger<MissingActivity> missingActivityLogger,
            IStringLocalizer<MissingActivity> missingActivityLocalizer,
            IClock clock
        )
        {
            _activityLibrary = activityLibrary;
            _workflowDefinitionStore = workflowDefinitionRepository;
            _workflowInstanceStore = workflowInstanceRepository;
            _workflowInstanceIdGenerator = workflowInstanceIdGenerator;
            _expressionEvaluator = expressionEvaluator;
            _scriptEvaluator = scriptEvaluator;
            _workflowContextHandlers = workflowContextHandlers;
            _workflowValueSerializers = workflowValueSerializers;
            _logger = logger;
            _workflowContextLogger = workflowContextLogger;
            _missingActivityLogger = missingActivityLogger;
            _missingActivityLocalizer = missingActivityLocalizer;
            _clock = clock;
        }

        public WorkflowInstance NewWorkflowInstance(WorkflowDefinition workflowDefinitionRecord, string correlationId = null)
        {
            var workflowInstanceRecord = new WorkflowInstance
            {
                WorkflowDefinitionId = workflowDefinitionRecord.WorkflowDefinitionId,
                Status = WorkflowStatus.Idle,
                State = JObject.FromObject(new WorkflowState
                {
                    ActivityStates = workflowDefinitionRecord.Activities.Select(x => x).ToDictionary(x => x.ActivityId, x => x.Properties)
                }),
                CorrelationId = correlationId,
                CreatedUtc = _clock.UtcNow
            };

            workflowInstanceRecord.WorkflowInstanceId = _workflowInstanceIdGenerator.GenerateUniqueId(workflowInstanceRecord);
            return workflowInstanceRecord;
        }

        public async Task<WorkflowExecutionContext> CreateWorkflowExecutionContextAsync(WorkflowDefinition workflowDefinitionRecord, WorkflowInstance workflowInstanceRecord, IDictionary<string, object> input = null)
        {
            var state = workflowInstanceRecord.State.ToObject<WorkflowState>();
            var activityQuery = await Task.WhenAll(workflowDefinitionRecord.Activities.Select(async x =>
            {
                var activityState = state.ActivityStates.ContainsKey(x.ActivityId) ? state.ActivityStates[x.ActivityId] : new JObject();
                return await CreateActivityExecutionContextAsync(x, activityState);
            }));
            var mergedInput = (await DeserializeAsync(state.Input)).Merge(input ?? new Dictionary<string, object>());
            var properties = await DeserializeAsync(state.Properties);
            var output = await DeserializeAsync(state.Output);
            var lastResult = await DeserializeAsync(state.LastResult);
            var executedActivities = state.ExecutedActivities;
            return new WorkflowExecutionContext(workflowDefinitionRecord, workflowInstanceRecord, mergedInput, output, properties, executedActivities, lastResult, activityQuery, _workflowContextHandlers.Resolve(), _expressionEvaluator, _scriptEvaluator, _workflowContextLogger);
        }

        public Task<ActivityContext> CreateActivityExecutionContextAsync(ActivityRecord activityRecord, JObject properties)
        {
            var activity = _activityLibrary.InstantiateActivity<IActivity>(activityRecord.Name, properties);

            if (activity == null)
            {
                _logger.LogWarning($"Requested activity '{activityRecord.Name}' does not exist in the library. This could indicate a changed name or a missing feature. Replacing it with MissingActivity.");
                activity = new MissingActivity(_missingActivityLocalizer, _missingActivityLogger, activityRecord);
            }

            var context = new ActivityContext
            {
                ActivityRecord = activityRecord,
                Activity = activity
            };

            return Task.FromResult(context);
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
            var workflowsToStart = await _workflowDefinitionStore.GetByStartActivityAsync(name);

            // And any running workflow paused on this kind of activity for the specified target.
            var workflowInstances = await _workflowInstanceStore.ListAsync(name, correlationId);

            // If no activity record is matching the event, do nothing.
            if (!workflowsToStart.Any() && !workflowInstances.Any())
            {
                return;
            }

            // Start new workflows.
            foreach (var workflowToStart in workflowsToStart)
            {
                // If this is a singleton workflow and there's already an instance, then skip.
                if (workflowToStart.IsSingleton && workflowInstances.Any(x => x.WorkflowDefinitionId == workflowToStart.WorkflowDefinitionId))
                {
                    continue;
                }

                var startActivity = workflowToStart.Activities.FirstOrDefault(x => x.IsStart && String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                if (startActivity != null)
                {
                    await StartWorkflowAsync(workflowToStart, startActivity, input, correlationId);
                }
            }

            // Resume pending workflows.
            foreach (var workflowInstance in workflowInstances)
            {
                var blockingActivities = workflowInstance.BlockingActivities.Where(x => x.Name == name).ToList();

                foreach (var blockingActivity in blockingActivities)
                {
                    await ResumeWorkflowAsync(workflowInstance, blockingActivity, input);
                }
            }
        }

        public async Task<WorkflowExecutionContext> ResumeWorkflowAsync(WorkflowInstance workflowInstance, BlockingActivity awaitingActivity, IDictionary<string, object> input = null)
        {
            var workflowDefinition = await _workflowDefinitionStore.GetAsync(workflowInstance.WorkflowDefinitionId);
            var activityRecord = workflowDefinition.Activities.SingleOrDefault(x => x.ActivityId == awaitingActivity.ActivityId);
            var workflowContext = await CreateWorkflowExecutionContextAsync(workflowDefinition, workflowInstance, input);

            workflowContext.Status = WorkflowStatus.Resuming;

            // Signal every activity that the workflow is about to be resumed.
            var cancellationToken = new CancellationToken();

            await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnInputReceivedAsync(workflowContext, input));
            await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowResumingAsync(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                workflowContext.Status = WorkflowStatus.Aborted;
            }
            else
            {
                // Check if the current activity can execute.
                var activityContext = workflowContext.GetActivity(activityRecord.ActivityId);
                if (await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
                {
                    // Signal every activity that the workflow is resumed.
                    await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowResumedAsync(workflowContext));

                    // Remove the blocking activity.
                    workflowContext.WorkflowInstanceRecord.BlockingActivities.Remove(awaitingActivity);

                    // Resume the workflow at the specified blocking activity.
                    await ExecuteWorkflowAsync(workflowContext, activityRecord);
                }
                else
                {
                    workflowContext.Status = WorkflowStatus.Halted;
                    return workflowContext;
                }
            }

            if (workflowContext.Status == WorkflowStatus.Finished && workflowDefinition.DeleteFinishedWorkflows)
            {
                await _workflowInstanceStore.DeleteAsync(workflowContext.WorkflowInstanceRecord);
            }
            else
            {
                await PersistAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<WorkflowExecutionContext> StartWorkflowAsync(WorkflowDefinition workflowDefinitionRecord, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null)
        {
            if (startActivity == null)
            {
                startActivity = workflowDefinitionRecord.Activities.FirstOrDefault(x => x.IsStart);

                if (startActivity == null)
                {
                    throw new InvalidOperationException($"Workflow with ID {workflowDefinitionRecord.Id} does not have a start activity.");
                }
            }

            // Create a new workflow instance.
            var workflowInstanceRecord = NewWorkflowInstance(workflowDefinitionRecord, correlationId);

            // Create a workflow context.
            var workflowContext = await CreateWorkflowExecutionContextAsync(workflowDefinitionRecord, workflowInstanceRecord, input);
            workflowContext.Status = WorkflowStatus.Starting;

            // Signal every activity about available input.
            await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnInputReceivedAsync(workflowContext, input));

            // Signal every activity that the workflow is about to start.
            var cancellationToken = new CancellationToken();
            await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowStartingAsync(workflowContext, cancellationToken));

            if (cancellationToken.IsCancellationRequested)
            {
                // Workflow is aborted.
                workflowContext.Status = WorkflowStatus.Aborted;
                return workflowContext;
            }
            else
            {
                // Check if the current activity can execute.
                var activityContext = workflowContext.GetActivity(startActivity.ActivityId);
                if (await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
                {
                    // Signal every activity that the workflow has started.
                    await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowStartedAsync(workflowContext));

                    // Execute the activity.
                    await ExecuteWorkflowAsync(workflowContext, startActivity);
                }
                else
                {
                    workflowContext.Status = WorkflowStatus.Idle;
                    return workflowContext;
                }
            }

            if (workflowContext.Status != WorkflowStatus.Finished || !workflowDefinitionRecord.DeleteFinishedWorkflows)
            {
                // Serialize state.
                await PersistAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowExecutionContext workflowContext, ActivityRecord activity)
        {
            var definition = workflowContext.WorkflowDefinitionRecord;
            var scheduled = new Stack<ActivityRecord>();
            var blocking = new List<ActivityRecord>();
            var isResuming = workflowContext.Status == WorkflowStatus.Resuming;
            var isFirstPass = true;

            workflowContext.Status = WorkflowStatus.Executing;
            scheduled.Push(activity);

            while (scheduled.Count > 0)
            {
                activity = scheduled.Pop();

                var activityContext = workflowContext.GetActivity(activity.ActivityId);

                // Signal every activity that the activity is about to be executed.
                var cancellationToken = new CancellationToken();
                await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnActivityExecutingAsync(workflowContext, activityContext, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                {
                    // Activity is aborted.
                    workflowContext.Status = WorkflowStatus.Aborted;
                    break;
                }

                IList<string> outcomes = new List<string>(0);

                try
                {
                    ActivityExecutionResult result;

                    if (!isResuming)
                    {
                        // Execute the current activity.
                        result = await activityContext.Activity.ExecuteAsync(workflowContext, activityContext);
                    }
                    else
                    {
                        // Resume the current activity.
                        result = await activityContext.Activity.ResumeAsync(workflowContext, activityContext);
                        isResuming = false;
                    }

                    if (result.IsHalted)
                    {
                        if (isFirstPass)
                        {
                            // Resume immediately when this is the first pass.
                            result = await activityContext.Activity.ResumeAsync(workflowContext, activityContext);
                            isFirstPass = false;
                            outcomes = result.Outcomes;

                            if (result.IsHalted)
                            {
                                // Block on this activity.
                                blocking.Add(activity);
                            }
                        }
                        else
                        {
                            // Block on this activity.
                            blocking.Add(activity);

                            continue;
                        }
                    }
                    else
                    {
                        outcomes = result.Outcomes;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unhandled error occurred while executing an activity. Workflow ID: {definition.Id}. Activity: {activityContext.ActivityRecord.ActivityId}, {activityContext.ActivityRecord.Name}. Putting the workflow in the faulted state.");
                    workflowContext.Fault(ex, activityContext);
                    return blocking.Distinct();
                }

                // Signal every activity that the activity is executed.
                await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnActivityExecutedAsync(workflowContext, activityContext));

                foreach (var outcome in outcomes)
                {
                    // Look for next activity in the graph.
                    var transition = definition.Transitions.FirstOrDefault(x => x.SourceActivityId == activity.ActivityId && x.SourceOutcomeName == outcome);

                    if (transition != null)
                    {
                        var destinationActivity = workflowContext.WorkflowDefinitionRecord.Activities.SingleOrDefault(x => x.ActivityId == transition.DestinationActivityId);
                        scheduled.Push(destinationActivity);
                    }
                }

                isFirstPass = false;
            }

            // Apply Distinct() as two paths could block on the same activity.
            var blockingActivities = blocking.Distinct().ToList();

            workflowContext.Status = blockingActivities.Any() || workflowContext.WorkflowInstanceRecord.BlockingActivities.Any() ? WorkflowStatus.Halted : WorkflowStatus.Finished;

            foreach (var blockingActivity in blockingActivities)
            {
                // Workflows containing with event activities could end up being blocked on the same activity.
                if (!workflowContext.WorkflowInstanceRecord.BlockingActivities.Any(x => x.ActivityId == blockingActivity.ActivityId))
                {
                    workflowContext.WorkflowInstanceRecord.BlockingActivities.Add(BlockingActivity.FromActivity(blockingActivity));
                }
            }

            return blockingActivities;
        }

        private async Task PersistAsync(WorkflowExecutionContext workflowContext)
        {
            var state = workflowContext.WorkflowInstanceRecord.State.ToObject<WorkflowState>();

            state.Input = await SerializeAsync(workflowContext.Input);
            state.Output = await SerializeAsync(workflowContext.Output);
            state.Properties = await SerializeAsync(workflowContext.Properties);
            state.LastResult = await SerializeAsync(workflowContext.LastResult);
            state.ExecutedActivities = workflowContext.ExecutedActivities.ToList();
            state.ActivityStates = workflowContext.Activities.ToDictionary(x => x.Key, x => x.Value.Activity.Properties);

            workflowContext.WorkflowInstanceRecord.State = JObject.FromObject(state);
            await _workflowInstanceStore.SaveAsync(workflowContext.WorkflowInstanceRecord);
        }

        /// <summary>
        /// Executes a specific action on all the activities of a workflow.
        /// </summary>
        private Task InvokeActivitiesAsync(WorkflowExecutionContext workflowContext, Func<ActivityContext, Task> action)
        {
            return workflowContext.Activities.Values.InvokeAsync(action, _logger);
        }

        private async Task<IDictionary<string, object>> SerializeAsync(IDictionary<string, object> dictionary)
        {
            var copy = new Dictionary<string, object>(dictionary.Count);
            foreach (var item in dictionary)
            {
                copy[item.Key] = await SerializeAsync(item.Value);
            }
            return copy;
        }

        private async Task<IDictionary<string, object>> DeserializeAsync(IDictionary<string, object> dictionary)
        {
            var copy = new Dictionary<string, object>(dictionary.Count);
            foreach (var item in dictionary)
            {
                copy[item.Key] = await DeserializeAsync(item.Value);
            }
            return copy;
        }

        private async Task<object> SerializeAsync(object value)
        {
            var context = new SerializeWorkflowValueContext(value);
            await _workflowValueSerializers.Resolve().InvokeAsync(x => x.SerializeValueAsync(context), _logger);
            return context.Output;
        }

        private async Task<object> DeserializeAsync(object value)
        {
            var context = new SerializeWorkflowValueContext(value);
            await _workflowValueSerializers.Resolve().InvokeAsync(x => x.DeserializeValueAsync(context), _logger);
            return context.Output;
        }
    }
}
