using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

public class WorkflowManager : IWorkflowManager
{
    // The maximum recursion depth is used to limit the number of Workflow (of any type) that a given
    // Workflow execution can trigger (directly or transitively) without reaching a blocking activity.
    private const int MaxRecursionDepth = 100;

    /// <summary>
    /// How many levels deep workflows can run workflows in a run (<see cref="StartChildWorkflowAsync"/>).
    /// </summary>
    public const int MaxChildWorkflowDepth = 16;

    private readonly IActivityLibrary _activityLibrary;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowTypeVersionStore _workflowTypeVersionStore;
    private readonly IWorkflowVariableTypeProvider _variableTypeProvider;
    private readonly IWorkflowStore _workflowStore;
    private readonly IWorkflowExecutionJournal _journal;
    private readonly IWorkflowDesignerNotifier _notifier;
    private readonly IWorkflowIdGenerator _workflowIdGenerator;
    private readonly Resolver<IEnumerable<IWorkflowValueSerializer>> _workflowValueSerializers;
    private readonly IWorkflowFaultHandler _workflowFaultHandler;
    private readonly IDistributedLock _distributedLock;
    private readonly ILogger _logger;
    private readonly ILogger<MissingActivity> _missingActivityLogger;
    private readonly IStringLocalizer<MissingActivity> _missingActivityLocalizer;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IClock _clock;

    private readonly Dictionary<string, int> _recursions = [];
    private int _currentRecursionDepth;
    private int _childWorkflowDepth;

    public WorkflowManager
    (
        IActivityLibrary activityLibrary,
        IWorkflowTypeStore workflowTypeRepository,
        IWorkflowTypeVersionStore workflowTypeVersionStore,
        IWorkflowVariableTypeProvider variableTypeProvider,
        IWorkflowStore workflowRepository,
        IWorkflowExecutionJournal journal,
        IWorkflowDesignerNotifier notifier,
        IWorkflowIdGenerator workflowIdGenerator,
        Resolver<IEnumerable<IWorkflowValueSerializer>> workflowValueSerializers,
        IWorkflowFaultHandler workflowFaultHandler,
        IDistributedLock distributedLock,
        ILogger<WorkflowManager> logger,
        ILogger<MissingActivity> missingActivityLogger,
        IStringLocalizer<MissingActivity> missingActivityLocalizer,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IClock clock)
    {
        _activityLibrary = activityLibrary;
        _workflowTypeStore = workflowTypeRepository;
        _workflowTypeVersionStore = workflowTypeVersionStore;
        _variableTypeProvider = variableTypeProvider;
        _workflowStore = workflowRepository;
        _journal = journal;
        _notifier = notifier;
        _workflowIdGenerator = workflowIdGenerator;
        _workflowValueSerializers = workflowValueSerializers;
        _workflowFaultHandler = workflowFaultHandler;
        _distributedLock = distributedLock;
        _logger = logger;
        _missingActivityLogger = missingActivityLogger;
        _missingActivityLocalizer = missingActivityLocalizer;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _clock = clock;
    }

    public Workflow NewWorkflow(WorkflowType workflowType, string correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        var workflow = new Workflow
        {
            WorkflowTypeId = workflowType.WorkflowTypeId,

            // The instance runs this version until it finishes, even when a new one is published.
            WorkflowTypeVersionId = workflowType.VersionId,
            Status = WorkflowStatus.Idle,
            State = JObject.FromObject(new WorkflowState
            {
                ActivityStates = workflowType.Activities.ToDictionary(x => x.ActivityId, x => x.Properties),
            }, _jsonSerializerOptions),
            CorrelationId = correlationId,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            CreatedUtc = _clock.UtcNow,
        };

        workflow.WorkflowId = _workflowIdGenerator.GenerateUniqueId(workflow);
        return workflow;
    }

    public async Task<WorkflowExecutionContext> CreateWorkflowExecutionContextAsync(WorkflowType workflowType, Workflow workflow, IDictionary<string, object> input = null)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        ArgumentNullException.ThrowIfNull(workflow);

        var state = workflow.State.ToObject<WorkflowState>(_jsonSerializerOptions);
        var activityQuery = await Task.WhenAll(workflowType.Activities.Select(x =>
        {
            if (!state.ActivityStates.TryGetValue(x.ActivityId, out var activityState))
            {
                activityState = [];
            }

            return CreateActivityExecutionContextAsync(x, activityState);
        }));

        var mergedInput = (await DeserializeAsync(state.Input)).Merge(input ?? new Dictionary<string, object>());
        var properties = await DeserializeAsync(state.Properties);
        var output = await DeserializeAsync(state.Output);
        var lastResult = await DeserializeAsync(state.LastResult);
        var executedActivities = state.ExecutedActivities;

        var workflowContext = new WorkflowExecutionContext(workflowType, workflow, mergedInput, output, properties, executedActivities, lastResult, activityQuery, _variableTypeProvider)
        {
            ExecutionSequence = state.ExecutionSequence,
        };

        // Declared variables that have no value yet start with their default value.
        workflowContext.Variables.ApplyDefaults();

        return workflowContext;
    }

    public Task<ActivityContext> CreateActivityExecutionContextAsync(ActivityRecord activityRecord, JsonObject properties)
    {
        ArgumentNullException.ThrowIfNull(activityRecord);

        var activity = _activityLibrary.InstantiateActivity<IActivity>(activityRecord.Name, properties);

        if (activity == null)
        {
            _logger.LogWarning("Requested activity '{ActivityName}' does not exist in the library. This could indicate a changed name or a missing feature. Replacing it with MissingActivity.", activityRecord.Name);
            activity = new MissingActivity(_missingActivityLocalizer, _missingActivityLogger, activityRecord);
        }

        var context = new ActivityContext
        {
            ActivityRecord = activityRecord,
            Activity = activity,
        };

        return Task.FromResult(context);
    }

    public async Task<IEnumerable<WorkflowExecutionContext>> TriggerEventAsync(string name, IDictionary<string, object> input = null, string correlationId = null, bool isExclusive = false, bool isAlwaysCorrelated = false)
    {
        var activity = _activityLibrary.GetActivityByName(name);
        if (activity == null)
        {
            _logger.LogError("Activity '{ActivityName}' was not found", name);
            return Array.Empty<WorkflowExecutionContext>();
        }

        var triggerdWorkflows = new List<WorkflowExecutionContext>();

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
                var context = await ResumeWorkflowAsync(haltedWorkflow, blockingActivity, input);
                triggerdWorkflows.Add(context);
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
            var context = await StartWorkflowAsync(workflowType, startActivity, input, correlationId);
            triggerdWorkflows.Add(context);
        }
        return triggerdWorkflows;
    }

    public async Task<WorkflowExecutionContext> ResumeWorkflowAsync(Workflow workflow, BlockingActivity awaitingActivity, IDictionary<string, object> input = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        ArgumentNullException.ThrowIfNull(awaitingActivity);

        // The instance resumes on the version it started on.
        var workflowType = await _workflowTypeVersionStore.GetWorkflowTypeAsync(
            await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId),
            workflow.WorkflowTypeVersionId);

        var activityRecord = workflowType.Activities.SingleOrDefault(x => x.ActivityId == awaitingActivity.ActivityId);
        var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow, input);

        if (activityRecord is null)
        {
            // For example an instance that started before versions existed, waiting on an activity that was removed since.
            _logger.LogWarning(
                "The workflow '{WorkflowId}' is waiting on the activity '{ActivityId}', which its definition doesn't have. Putting the workflow in the faulted state.",
                workflow.WorkflowId,
                awaitingActivity.ActivityId);

            workflowContext.Status = WorkflowStatus.Faulted;
            workflow.FaultMessage = $"The activity '{awaitingActivity.ActivityId}' the workflow is waiting on doesn't exist in its definition.";
            await PersistAsync(workflowContext);

            return workflowContext;
        }

        workflowContext.Status = WorkflowStatus.Resuming;

        // Signal every activity that the workflow is about to be resumed.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnInputReceivedAsync(workflowContext, input));
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowResumingAsync(workflowContext, workflowContext.CancellationToken));

        if (workflowContext.CancellationToken.IsCancellationRequested)
        {
            return workflowContext;
        }

        // Check if the current activity can execute.
        var activityContext = workflowContext.GetActivity(activityRecord.ActivityId);
        if (!await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
        {
            workflowContext.Status = WorkflowStatus.Halted;

            return workflowContext;
        }

        // Signal every activity that the workflow is resumed.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowResumedAsync(workflowContext));

        // Remove the blocking activity.
        workflowContext.Workflow.BlockingActivities.Remove(awaitingActivity);

        // Resume the workflow at the specified blocking activity.
        await ExecuteWorkflowAsync(workflowContext, activityRecord);

        if (workflowContext.Status == WorkflowStatus.Finished && workflowType.DeleteFinishedWorkflows)
        {
            await _workflowStore.DeleteAsync(workflowContext.Workflow);
            await NotifyInstanceChangedAsync(workflowContext, isDeleted: true);
        }
        else
        {
            await PersistAsync(workflowContext);
        }

        await ResumeParentAsync(workflowContext);

        return workflowContext;
    }

    /// <inheritdoc />
    public async Task<WorkflowExecutionContext> RetryActivityAsync(Workflow workflow, string activityId)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrEmpty(activityId);

        if (workflow.Status != WorkflowStatus.Faulted)
        {
            throw new InvalidOperationException($"The workflow '{workflow.WorkflowId}' isn't faulted, so it can't be retried.");
        }

        var currentType = await _workflowTypeStore.GetAsync(workflow.WorkflowTypeId)
            ?? throw new InvalidOperationException($"The workflow type '{workflow.WorkflowTypeId}' of the workflow '{workflow.WorkflowId}' doesn't exist.");

        // The instance runs again on the version it started on.
        var workflowType = await _workflowTypeVersionStore.GetWorkflowTypeAsync(currentType, workflow.WorkflowTypeVersionId);
        var activity = workflowType.Activities.SingleOrDefault(x => x.ActivityId == activityId)
            ?? throw new ArgumentException($"The definition of the workflow '{workflow.WorkflowId}' doesn't have the activity '{activityId}'.", nameof(activityId));

        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowLockAsync(workflow);

        if (!locked)
        {
            _logger.LogWarning("The workflow '{WorkflowId}' wasn't retried: another process holds its lock.", workflow.WorkflowId);

            return null;
        }

        await using var acquiredLock = locker;

        var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow);
        workflow.FaultMessage = null;

        await ExecuteWorkflowAsync(workflowContext, activity);

        if (workflowContext.Status == WorkflowStatus.Finished && workflowType.DeleteFinishedWorkflows)
        {
            await _workflowStore.DeleteAsync(workflow);
            await NotifyInstanceChangedAsync(workflowContext, isDeleted: true);
        }
        else
        {
            await PersistAsync(workflowContext);
        }

        await ResumeParentAsync(workflowContext);

        return workflowContext;
    }

    public async Task<WorkflowExecutionContext> RestartWorkflowAsync(WorkflowType workflowType, IDictionary<string, object> input = null, string correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        var startActivity = workflowType.Activities?.FirstOrDefault(x => x.IsStart)
            ?? throw new InvalidOperationException($"Workflow with ID {workflowType.Id} does not have a start activity.");

        // Create a new workflow instance.
        var workflow = NewWorkflow(workflowType, correlationId);

        // Create a workflow context.
        var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow, input);
        workflowContext.Status = WorkflowStatus.Starting;
        ApplyInputs(workflowContext, input);

        // Signal every activity that the workflow is about to start.
        // This should be called prior OnInputReceivedAsync.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowRestartingAsync(workflowContext, workflowContext.CancellationToken));

        // Signal every activity about available input.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnInputReceivedAsync(workflowContext, input));

        if (workflowContext.CancellationToken.IsCancellationRequested)
        {
            return workflowContext;
        }

        // Check if the current activity can execute.
        var activityContext = workflowContext.GetActivity(startActivity.ActivityId);
        if (!await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
        {
            workflowContext.Status = WorkflowStatus.Idle;

            return workflowContext;
        }

        // Signal every activity that the workflow has started.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowRestartedAsync(workflowContext));

        // Execute the activity.
        await ExecuteWorkflowAsync(workflowContext, startActivity);

        if (workflowContext.Status != WorkflowStatus.Finished || !workflowType.DeleteFinishedWorkflows)
        {
            // Serialize state.
            await PersistAsync(workflowContext);
        }

        return workflowContext;
    }

    public async Task<WorkflowExecutionContext> StartWorkflowAsync(WorkflowType workflowType, ActivityRecord startActivity = null, IDictionary<string, object> input = null, string correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(workflowType);

        startActivity ??= workflowType.Activities?.FirstOrDefault(x => x.IsStart)
            ?? throw new InvalidOperationException($"Workflow with ID {workflowType.Id} does not have a start activity.");

        return await StartWorkflowCoreAsync(workflowType, startActivity, input, correlationId, initialize: null);
    }

    /// <inheritdoc />
    public async Task<WorkflowExecutionContext> StartChildWorkflowAsync(WorkflowType workflowType, WorkflowExecutionContext parentContext, string parentActivityId, IDictionary<string, object> input = null)
    {
        ArgumentNullException.ThrowIfNull(workflowType);
        ArgumentNullException.ThrowIfNull(parentContext);
        ArgumentException.ThrowIfNullOrEmpty(parentActivityId);

        if (_childWorkflowDepth >= MaxChildWorkflowDepth)
        {
            throw new InvalidOperationException($"Workflows can run other workflows {MaxChildWorkflowDepth} levels deep at most.");
        }

        var startActivity = workflowType.Activities?.FirstOrDefault(x => x.IsStart && x.Name == nameof(StartedByWorkflowEvent))
            ?? workflowType.Activities?.FirstOrDefault(x => x.IsStart)
            ?? throw new InvalidOperationException($"The workflow '{workflowType.Name}' doesn't have a start activity.");

        _childWorkflowDepth++;

        try
        {
            return await StartWorkflowCoreAsync(workflowType, startActivity, input, correlationId: null, workflow =>
            {
                workflow.ParentWorkflowId = parentContext.WorkflowId;
                workflow.ParentActivityId = parentActivityId;
            });
        }
        finally
        {
            _childWorkflowDepth--;
        }
    }

    private async Task<WorkflowExecutionContext> StartWorkflowCoreAsync(
        WorkflowType workflowType,
        ActivityRecord startActivity,
        IDictionary<string, object> input,
        string correlationId,
        Action<Workflow> initialize)
    {
        // Create a new workflow instance.
        var workflow = NewWorkflow(workflowType, correlationId);
        initialize?.Invoke(workflow);

        // Create a workflow context.
        var workflowContext = await CreateWorkflowExecutionContextAsync(workflowType, workflow, input);
        workflowContext.Status = WorkflowStatus.Starting;
        ApplyInputs(workflowContext, input);

        // Signal every activity about available input.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnInputReceivedAsync(workflowContext, input));

        // Signal every activity that the workflow is about to start.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowStartingAsync(workflowContext, workflowContext.CancellationToken));

        if (workflowContext.CancellationToken.IsCancellationRequested)
        {
            return workflowContext;
        }

        // Check if the current activity can execute.
        var activityContext = workflowContext.GetActivity(startActivity.ActivityId);
        if (!await activityContext.Activity.CanExecuteAsync(workflowContext, activityContext))
        {
            workflowContext.Status = WorkflowStatus.Idle;

            return workflowContext;
        }

        // Signal every activity that the workflow has started.
        await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnWorkflowStartedAsync(workflowContext));

        // Execute the activity.
        await ExecuteWorkflowAsync(workflowContext, startActivity);

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
        var recursionDecremented = false;
        try
        {
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
                await InvokeActivitiesAsync(workflowContext, x => x.Activity.OnActivityExecutingAsync(workflowContext, activityContext, workflowContext.CancellationToken));

                if (workflowContext.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var outcomes = Enumerable.Empty<string>();
                var startedUtc = _clock.UtcNow;
                var resumed = isResuming;

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
                            workflowContext.RecordExecution(activityContext, WorkflowExecutionRecordStatus.Halted, [], startedUtc, _clock.UtcNow, resumed);

                            continue;
                        }
                    }
                    else
                    {
                        outcomes = result.Outcomes;
                    }

                    // Once the activity has run, its bound outputs are written to their variables.
                    if (!result.IsHalted)
                    {
                        ApplyOutputBindings(workflowContext, activityContext);
                    }

                    workflowContext.RecordExecution(
                        activityContext,
                        result.IsHalted ? WorkflowExecutionRecordStatus.Halted : WorkflowExecutionRecordStatus.Completed,
                        result.IsHalted ? [] : outcomes,
                        startedUtc,
                        _clock.UtcNow,
                        resumed);
                }
                catch (Exception ex)
                {
                    workflowContext.RecordExecution(activityContext, WorkflowExecutionRecordStatus.Faulted, [], startedUtc, _clock.UtcNow, resumed, ex.Message);

                    _logger.LogError(ex, "An unhandled error occurred while executing an activity. Workflow ID: '{WorkflowTypeId}'. Activity: '{ActivityId}', '{ActivityName}'. Putting the workflow in the faulted state.", workflowType.Id, activityContext.ActivityRecord.ActivityId, activityContext.ActivityRecord.Name);
                    workflowContext.Fault(ex, activityContext);

                    DecrementRecursion(workflowContext.Workflow);
                    recursionDecremented = true;

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

            workflowContext.Status = blockingActivities.Count > 0 || workflowContext.Workflow.BlockingActivities.Count > 0 ? WorkflowStatus.Halted : WorkflowStatus.Finished;

            foreach (var blockingActivity in blockingActivities)
            {
                // Workflows containing event activities could end up being blocked on the same activity.
                if (!workflowContext.Workflow.BlockingActivities.Any(x => x.ActivityId == blockingActivity.ActivityId))
                {
                    workflowContext.Workflow.BlockingActivities.Add(BlockingActivity.FromActivity(blockingActivity));
                }
            }

            return blockingActivities;
        }
        finally
        {
            if (!recursionDecremented)
            {
                // Decrement the workflow scope recursion.
                DecrementRecursion(workflowContext.Workflow);
            }
        }
    }

    // A child instance (StartChildWorkflowAsync) that finished or faulted in a later run than the one that started it
    // resumes its parent's activity, when the parent waits on it.
    private async Task ResumeParentAsync(WorkflowExecutionContext childContext)
    {
        var child = childContext.Workflow;

        if (string.IsNullOrEmpty(child.ParentWorkflowId) || childContext.Status is not (WorkflowStatus.Finished or WorkflowStatus.Faulted))
        {
            return;
        }

        // The parent runs in this scope: its activity reads the result itself.
        if (_recursions.TryGetValue(child.ParentWorkflowId, out var count) && count > 0)
        {
            return;
        }

        var parent = await _workflowStore.GetAsync(child.ParentWorkflowId);
        var blockingActivity = parent?.BlockingActivities.FirstOrDefault(x => x.ActivityId == child.ParentActivityId);

        if (blockingActivity is null)
        {
            return;
        }

        (var locker, var locked) = await _distributedLock.TryAcquireWorkflowLockAsync(parent);

        if (!locked)
        {
            _logger.LogWarning("The workflow '{WorkflowId}' wasn't resumed when its child '{ChildWorkflowId}' ended: another process holds its lock.", parent.WorkflowId, child.WorkflowId);

            return;
        }

        await using var acquiredLock = locker;

        await ResumeWorkflowAsync(parent, blockingActivity, new Dictionary<string, object>
        {
            [ChildWorkflowResult.InputKey] = ChildWorkflowResult.From(childContext),
        });
    }

    // The input variables take the input values of the same name.
    private void ApplyInputs(WorkflowExecutionContext workflowContext, IDictionary<string, object> input)
    {
        foreach (var name in workflowContext.Variables.ApplyInputs(input))
        {
            _logger.LogWarning("The input '{Name}' of the workflow '{WorkflowId}' doesn't convert to the type of its variable, which keeps its default value.", name, workflowContext.Workflow.WorkflowId);
        }
    }

    // Writes the outputs the activity set to the variables they are bound to. A value that doesn't convert to its
    // variable's type throws, which faults the workflow.
    private static void ApplyOutputBindings(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var bindings = activityContext.Activity.Properties.GetOutputBindings();

        if (bindings.Count == 0)
        {
            return;
        }

        var outputs = workflowContext.GetActivityOutputs(activityContext.ActivityRecord.ActivityId);

        foreach (var (output, variable) in bindings)
        {
            if (outputs.TryGetValue(output, out var value))
            {
                workflowContext.Variables.Set(variable, value);
            }
        }
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
        var state = workflowContext.Workflow.State.ToObject<WorkflowState>(_jsonSerializerOptions);

        state.Input = await SerializeAsync(workflowContext.Input);
        state.Output = await SerializeAsync(workflowContext.Output);
        state.Properties = await SerializeAsync(workflowContext.Properties);
        state.LastResult = await SerializeAsync(workflowContext.LastResult);
        // Oldest first, so the stack built from it on the next run has the most recent entry on top.
        state.ExecutedActivities = workflowContext.ExecutedActivities.Reverse().ToList();
        state.ExecutionSequence = workflowContext.ExecutionSequence;
        state.ActivityStates = workflowContext.Activities.ToDictionary(x => x.Key, x => x.Value.Activity.Properties);

        workflowContext.Workflow.State = JObject.FromObject(state, _jsonSerializerOptions);
        await _workflowStore.SaveAsync(workflowContext.Workflow);

        if (_journal.IsEnabled && workflowContext.JournalRecords.Count > 0)
        {
            await _journal.SaveAsync(workflowContext.Workflow.WorkflowId, workflowContext.JournalRecords.ToList());
        }

        workflowContext.JournalRecords.Clear();

        await NotifyInstanceChangedAsync(workflowContext, isDeleted: false);
    }

    private Task NotifyInstanceChangedAsync(WorkflowExecutionContext workflowContext, bool isDeleted)
        => _notifier.InstanceChangedAsync(new WorkflowInstanceChange
        {
            WorkflowId = workflowContext.Workflow.WorkflowId,
            WorkflowTypeId = workflowContext.Workflow.WorkflowTypeId,
            Status = workflowContext.Status,
            IsDeleted = isDeleted,
        });

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
