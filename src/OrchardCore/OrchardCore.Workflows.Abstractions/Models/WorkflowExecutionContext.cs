using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models;

public sealed class WorkflowExecutionContext : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public WorkflowExecutionContext
    (
        WorkflowType workflowType,
        Workflow workflow,
        IDictionary<string, object> input,
        IDictionary<string, object> output,
        IDictionary<string, object> properties,
        IList<ExecutedActivity> executedActivities,
        object lastResult,
        IEnumerable<ActivityContext> activities,
        IWorkflowVariableTypeProvider variableTypes = null
    )
    {
        Input = input ?? new Dictionary<string, object>();
        Output = output ?? new Dictionary<string, object>();
        Properties = properties ?? new Dictionary<string, object>();
        ExecutedActivities = new Stack<ExecutedActivity>(executedActivities ?? []);
        LastResult = lastResult;
        WorkflowType = workflowType;
        Workflow = workflow;
        Activities = activities.ToDictionary(x => x.ActivityRecord.ActivityId);
        Variables = new WorkflowVariables(Properties, workflowType?.Variables, variableTypes is null ? null : variableTypes.Get);
    }

    // The outputs the activities set while this context runs, by activity id, then output name.
    private readonly Dictionary<string, Dictionary<string, object>> _activityOutputs = [];

    public Workflow Workflow { get; }
    public WorkflowType WorkflowType { get; }

    /// <summary>
    /// Sets an output of an activity (see <see cref="Activities.IActivityOutputs"/>). Once the activity has run, the
    /// engine writes it to the variable it is bound to, if any. An activity context without a record (an activity
    /// run outside of a workflow) is ignored.
    /// </summary>
    public void SetActivityOutput(ActivityContext activityContext, string name, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var activityId = activityContext?.ActivityRecord?.ActivityId;

        if (string.IsNullOrEmpty(activityId))
        {
            return;
        }

        if (!_activityOutputs.TryGetValue(activityId, out var outputs))
        {
            _activityOutputs[activityId] = outputs = [];
        }

        outputs[name] = value;
    }

    /// <summary>
    /// Returns the outputs an activity set while this context ran.
    /// </summary>
    public IReadOnlyDictionary<string, object> GetActivityOutputs(string activityId)
        => !string.IsNullOrEmpty(activityId) && _activityOutputs.TryGetValue(activityId, out var outputs) ? outputs : new Dictionary<string, object>();

    /// <summary>
    /// The variables of the workflow: the declared ones, typed, over <see cref="Properties"/>.
    /// </summary>
    public WorkflowVariables Variables { get; }
    public IDictionary<string, ActivityContext> Activities { get; }

    public string WorkflowId
    {
        get => Workflow.WorkflowId;
    }

    public string CorrelationId
    {
        get => Workflow.CorrelationId;
        set => Workflow.CorrelationId = value;
    }

    /// <summary>
    /// A dictionary of re-hydrated values provided by the initiator of the workflow.
    /// </summary>
    public IDictionary<string, object> Input { get; }

    /// <summary>
    /// A dictionary of re-hydrated values provided to the initiator of the workflow.
    /// </summary>
    public IDictionary<string, object> Output { get; }

    /// <summary>
    /// A dictionary of re-hydrated values provided by the workflow activities.
    /// </summary>
    public IDictionary<string, object> Properties { get; }

    /// <summary>
    /// The value returned from the previous activity, if any.
    /// </summary>
    public object LastResult { get; set; }

    public WorkflowStatus Status
    {
        get => Workflow.Status;
        set => Workflow.Status = value;
    }

    /// <summary>
    /// Keeps track of which activities executed in which order.
    /// </summary>
    public Stack<ExecutedActivity> ExecutedActivities { get; set; }

    /// <summary>
    /// Gets a cancellation token that gets signaled when the workflow is cancelled.
    /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public ActivityContext GetActivity(string activityId)
    {
        return Activities[activityId];
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }

    public void Cancel(string reason = null)
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();

            // Workflow is aborted.
            Workflow.Status = WorkflowStatus.Aborted;
            Workflow.FaultMessage = reason;
        }
    }

    public void Fault(Exception exception, ActivityContext _)
    {
        Workflow.Status = WorkflowStatus.Faulted;
        Workflow.FaultMessage = exception.Message;
    }

    public IEnumerable<Transition> GetInboundTransitions(string activityId)
    {
        return WorkflowType.Transitions.Where(x => x.DestinationActivityId == activityId).ToList();
    }

    public IEnumerable<Transition> GetOutboundTransitions(string activityId)
    {
        return WorkflowType.Transitions.Where(x => x.SourceActivityId == activityId).ToList();
    }

    /// <summary>
    /// Returns the full path of incoming activities.
    /// </summary>
    public IEnumerable<string> GetInboundActivityPath(string activityId)
    {
        return GetInboundActivityPathInternal(activityId, activityId).Distinct().ToList();
    }

    private IEnumerable<string> GetInboundActivityPathInternal(string activityId, string startingPointActivityId)
    {
        foreach (var transition in GetInboundTransitions(activityId))
        {
            // Circuit breaker: Detect workflows that implement repeating flows to prevent an infinite loop here.
            if (transition.SourceActivityId == startingPointActivityId)
            {
                yield break;
            }
            else
            {
                yield return transition.SourceActivityId;

                foreach (var parentActivityId in GetInboundActivityPathInternal(transition.SourceActivityId, startingPointActivityId).Distinct())
                {
                    yield return parentActivityId;
                }
            }
        }
    }
}
