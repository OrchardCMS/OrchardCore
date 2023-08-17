using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowManager : IWorkflowManager
    {
        // The maximum recursion depth is used to limit the number of Workflow (of any type) that a given
        // Workflow execution can trigger (directly or transitively) without reaching a blocking activity.
        private const int MaxRecursionDepth = 100;

        private readonly IActivityLibrary _activityLibrary;
        private readonly IWorkflowTypeStore _workflowTypeStore;
        private readonly IWorkflowStore _workflowStore;
        private readonly IWorkflowIdGenerator _workflowIdGenerator;
        private readonly Resolver<IEnumerable<IWorkflowValueSerializer>> _workflowValueSerializers;
        private readonly IWorkflowFaultHandler _workflowFaultHandler;
        private readonly IDistributedLock _distributedLock;
        private readonly ILogger _logger;
        private readonly ILogger<MissingActivity> _missingActivityLogger;
        private readonly IStringLocalizer<MissingActivity> _missingActivityLocalizer;
        private readonly IClock _clock;

        private readonly Dictionary<string, int> _recursions = new();
        private int _currentRecursionDepth;

        public WorkflowManager
        (
            IActivityLibrary activityLibrary,
            IWorkflowTypeStore workflowTypeRepository,
            IWorkflowStore workflowRepository,
            IWorkflowIdGenerator workflowIdGenerator,
            Resolver<IEnumerable<IWorkflowValueSerializer>> workflowValueSerializers,
            IWorkflowFaultHandler workflowFaultHandler,
            IDistributedLock distributedLock,
            ILogger<WorkflowManager> logger,
            ILogger<MissingActivity> missingActivityLogger,
            IStringLocalizer<MissingActivity> missingActivityLocalizer,
            IClock clock)
        {
            _activityLibrary = activityLibrary;
            _workflowTypeStore = workflowTypeRepository;
            _workflowStore = workflowRepository;
            _workflowIdGenerator = workflowIdGenerator;
            _workflowValueSerializers = workflowValueSerializers;
            _workflowFaultHandler = workflowFaultHandler;
            _distributedLock = distributedLock;
            _logger = logger;
            _missingActivityLogger = missingActivityLogger;
            _missingActivityLocalizer = missingActivityLocalizer;
            _clock = clock;
        }

        public Workflow NewWorkflow(WorkflowType workflowType, string correlationId = null)
        {
            var workflow = new Workflow
            {
                WorkflowTypeId = workflowType.WorkflowTypeId,
                Status = WorkflowStatus.Idle,
                State = JObject.FromObject(new WorkflowState
                {
                    ActivityStates = workflowType.Activities.Select(x => x).ToDictionary(x => x.ActivityId, x => x.Properties)
                }),
                CorrelationId = correlationId,
                LockTimeout = workflowType.LockTimeout,
                LockExpiration = workflowType.LockExpiration,
                CreatedUtc = _clock.UtcNow
            };

            workflow.WorkflowId = _workflowIdGenerator.GenerateUniqueId(workflow);
            return workflow;
        }

        public async Task<WorkflowExecutionContext> CreateWorkflowExecutionContextAsync(WorkflowType workflowType, Workflow workflow, IDictionary<string, object> input = null)
        {
            var state = workflow.State.ToObject<WorkflowState>();
            var activityQuery = await Task.WhenAll(workflowType.Activities.Select(x =>
            {
                if (!state.ActivityStates.TryGetValue(x.ActivityId, out var activityState))
                {
                    activityState = new JObject();
                }

                return CreateActivityExecutionContextAsync(x, activityState);
            }));

            var mergedInput = (await DeserializeAsync(state.Input)).Merge(input ?? new Dictionary<string, object>());
            var properties = await DeserializeAsync(state.Properties);
            var output = await DeserializeAsync(state.Output);
            var lastResult = await DeserializeAsync(state.LastResult);
            var executedActivities = state.ExecutedActivities;
            return new WorkflowExecutionContext(workflowType, workflow, mergedInput, output, properties, executedActivities, lastResult, activityQuery);
        }

        public Task<ActivityContext> CreateActivityExecutionContextAsync(ActivityRecord activityRecord, JObject properties)
        {
            var activity = _activityLibrary.InstantiateActivity<IActivity>(activityRecord.Name, properties);

            if (activity == null)
            {
                _logger.LogWarning("Requested activity '{ActivityName}' does not exist in the library. This could indicate a changed name or a missing feature. Replacing it with MissingActivity.", activityRecord.Name);
                activity = new MissingActivity(_missingActivityLocalizer, _missingActivityLogger, activityRecord);
            }

            var context = new ActivityContext
            {
                ActivityRecord = activityRecord,
                Activity = activity
            };

            return Task.FromResult(context);
        }

        public async Task TriggerEventAsync(string name, IDictionary<string, object> input = null, string correlationId = null, bool isExclusive = false, bool isAlwaysCorrelated = false)
        {
            var activity = _activityLibrary.GetActivityByName(name);
            if (activity == null)
            {
                _logger.LogError("Activity '{ActivityName}' was not found", name);
                return;
            }

            // Resume workflow instances halted on this kind of activity for the specified target.
            var haltedWorkflows = await _workflowStore.ListByActivityNameAsync(name, correlationId, isAlwaysCorrelated);
            foreach (var workflow in haltedWorkflows)
            {
                // Don't allow scope recursion per workflow instance id.
                if (_recursions.TryGetValue(workflow.WorkflowId, out var count) && count > 0)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Don't allow scope recursion per workflow instance id: '{Workflow}'.", workflow.WorkflowId);
                    }

                    continue;
                }

                // If atomic, try to acquire a lock per workflow instance.
                (var locker, var locked) = await _distributedLock.TryAcquireWorkflowLockAsync(workflow);
                if (!locked)
                {
                    continue;
                }

                await using var acquiredLock = locker;

                // If atomic, check if the workflow still exists and is still correlated.
                var haltedWorkflow = workflow.IsAtomic ? await _workflowStore.GetAsync(workflow.Id) : workflow;
                if (haltedWorkflow == null || (!isAlwaysCorrelated && haltedWorkflow.CorrelationId != (correlationId ?? "")))
                {
                    continue;
                }

                // Check the max recursion depth of workflow executions.
                if (_currentRecursionDepth > MaxRecursionDepth)
                {
                    _logger.LogError("The max recursion depth of 'Workflow' executions has been reached.");
                    break;
                }

                var blockingActivities = haltedWorkflow.BlockingActivities.Where(x => x.Name == name).ToArray();
                foreach (var blockingActivity in blockingActivities)
                {
                    await ResumeWorkflowAsync(haltedWorkflow, blockingActivity, input);
                }
            }

            // Start new workflows whose types have a corresponding starting activity.
            var workflowTypesToStart = await _workflowTypeStore.GetByStartActivityAsync(name);
            foreach (var workflowType in workflowTypesToStart)
            {
                // Don't allow scope recursion per workflow type id.
                if (_recursions.TryGetValue(workflowType.WorkflowTypeId, out var count) && count > 0)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Don't allow scope recursion per workflow type: '{WorkflowType}'.", workflowType.Name);
                    }

                    continue;
                }

                // If a singleton or the event is exclusive, try to acquire a lock per workflow type.
                (var locker, var locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType, isExclusive);
                if (!locked)
                {
                    continue;
                }

                await using var acquiredLock = locker;

                // Check if this is a workflow singleton and there's already an halted instance on any activity.
                if (workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
                {
                    continue;
                }

                // Check if the event is exclusive and there's already a correlated instance halted on a starting activity of this type.
                if (isExclusive && (await _workflowStore.ListAsync(workflowType.WorkflowTypeId, name, correlationId, isAlwaysCorrelated))
                    .Any(x => x.BlockingActivities.Any(x => x.Name == name && x.IsStart)))
                {
                    continue;
                }

                // Check the max recursion depth of workflow executions.
                if (_currentRecursionDepth > MaxRecursionDepth)
                {
                    _logger.LogError("The max recursion depth of 'Workflow' executions has been reached.");
                    break;
                }

                var startActivity = workflowType.Activities.First(x => x.IsStart && x.Name == name);
                await StartWorkflowAsync(workflowType, startActivity, input, correlationId);
            }
        }

        public async Task<WorkflowExecutionContext> ResumeWorkflowAsync(Workflow workflow, BlockingActivity awaitingActivity, IDictionary<string, object> input = null)
        {
            var workflowType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId);
            var activityRecord = workflowType.Activities.SingleOrDefault(x => x.ActivityId == awaitingActivity.ActivityId);
            var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow, input);

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
                    workflowContext.Workflow.BlockingActivities.Remove(awaitingActivity);

                    // Resume the workflow at the specified blocking activity.
                    await ExecuteWorkflowAsync(workflowContext, activityRecord);
                }
                else
                {
                    workflowContext.Status = WorkflowStatus.Halted;
                    return workflowContext;
                }
            }

            if (workflowContext.Status == WorkflowStatus.Finished && workflowType.DeleteFinishedWorkflows)
            {
                await _workflowStore.DeleteAsync(workflowContext.Workflow);
            }
            else
            {
                await PersistAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<WorkflowExecutionContext> StartWorkflowAsync(WorkflowType workflowType, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null)
        {
            if (startActivity == null)
            {
                startActivity = workflowType.Activities.FirstOrDefault(x => x.IsStart);

                if (startActivity == null)
                {
                    throw new InvalidOperationException($"Workflow with ID {workflowType.Id} does not have a start activity.");
                }
            }

            // Create a new workflow instance.
            var workflow = NewWorkflow(workflowType, correlationId);

            // Create a workflow context.
            var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow, input);
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

            if (workflowContext.Status != WorkflowStatus.Finished || !workflowType.DeleteFinishedWorkflows)
            {
                // Serialize state.
                await PersistAsync(workflowContext);
            }

            return workflowContext;
        }

        public async Task<IEnumerable<ActivityRecord>> ExecuteWorkflowAsync(WorkflowExecutionContext workflowContext, ActivityRecord activity)
        {
            // Prevent scope recursion per workflow.
            IncrementRecursion(workflowContext.Workflow);

            var workflowType = workflowContext.WorkflowType;
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

                var outcomes = Enumerable.Empty<string>();

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
                    _logger.LogError(ex, "An unhandled error occurred while executing an activity. Workflow ID: '{WorkflowTypeId}'. Activity: '{ActivityId}', '{ActivityName}'. Putting the workflow in the faulted state.", workflowType.Id, activityContext.ActivityRecord.ActivityId, activityContext.ActivityRecord.Name);
                    workflowContext.Fault(ex, activityContext);

                    // Decrement the workflow scope recursion count.
                    DecrementRecursion(workflowContext.Workflow);

                    await _workflowFaultHandler.OnWorkflowFaultAsync(this, workflowContext, activityContext, ex);

                    return blocking.Distinct();
                }

                // Signal every activity that the activity is executed.
                await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnActivityExecutedAsync(workflowContext, activityContext));

                foreach (var outcome in outcomes)
                {
                    // Look for next activity in the graph.
                    var transition = workflowType.Transitions.FirstOrDefault(x => x.SourceActivityId == activity.ActivityId && x.SourceOutcomeName == outcome);

                    if (transition != null)
                    {
                        var destinationActivity = workflowContext.WorkflowType.Activities.SingleOrDefault(x => x.ActivityId == transition.DestinationActivityId);

                        // Check that the activity doesn't point to itself.
                        if (destinationActivity != activity)
                        {
                            scheduled.Push(destinationActivity);
                        }
                    }
                }

                isFirstPass = false;
            }

            // Apply Distinct() as two paths could block on the same activity.
            var blockingActivities = blocking.Distinct().ToList();

            workflowContext.Status = blockingActivities.Any() || workflowContext.Workflow.BlockingActivities.Any() ? WorkflowStatus.Halted : WorkflowStatus.Finished;

            foreach (var blockingActivity in blockingActivities)
            {
                // Workflows containing event activities could end up being blocked on the same activity.
                if (!workflowContext.Workflow.BlockingActivities.Any(x => x.ActivityId == blockingActivity.ActivityId))
                {
                    workflowContext.Workflow.BlockingActivities.Add(BlockingActivity.FromActivity(blockingActivity));
                }
            }

            // Decrement the workflow scope recursion.
            DecrementRecursion(workflowContext.Workflow);

            return blockingActivities;
        }

        private void IncrementRecursion(Workflow workflow)
        {
            _recursions[workflow.WorkflowId] = _recursions.TryGetValue(workflow.WorkflowId, out var count) ? ++count : 1;
            _recursions[workflow.WorkflowTypeId] = _recursions.TryGetValue(workflow.WorkflowTypeId, out count) ? ++count : 1;
            _currentRecursionDepth++;
        }

        private void DecrementRecursion(Workflow workflow)
        {
            _recursions[workflow.WorkflowId]--;
            _recursions[workflow.WorkflowTypeId]--;
            _currentRecursionDepth--;
        }

        private async Task PersistAsync(WorkflowExecutionContext workflowContext)
        {
            var state = workflowContext.Workflow.State.ToObject<WorkflowState>();

            state.Input = await SerializeAsync(workflowContext.Input);
            state.Output = await SerializeAsync(workflowContext.Output);
            state.Properties = await SerializeAsync(workflowContext.Properties);
            state.LastResult = await SerializeAsync(workflowContext.LastResult);
            state.ExecutedActivities = workflowContext.ExecutedActivities.ToList();
            state.ActivityStates = workflowContext.Activities.ToDictionary(x => x.Key, x => x.Value.Activity.Properties);

            workflowContext.Workflow.State = JObject.FromObject(state);
            await _workflowStore.SaveAsync(workflowContext.Workflow);
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
            await _workflowValueSerializers.Resolve().InvokeAsync((s, context) => s.SerializeValueAsync(context), context, _logger);
            return context.Output;
        }

        private async Task<object> DeserializeAsync(object value)
        {
            var context = new SerializeWorkflowValueContext(value);
            await _workflowValueSerializers.Resolve().InvokeAsync((s, context) => s.DeserializeValueAsync(context), context, _logger);
            return context.Output;
        }
    }
}
