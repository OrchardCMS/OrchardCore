using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities;

public abstract class Activity : Entity, IActivity
{
    public abstract string Name { get; }
    public abstract LocalizedString DisplayText { get; }
    public abstract LocalizedString Category { get; }
    public virtual bool HasEditor => true;

    public virtual ValueTask<IEnumerable<Outcome>> GetPossibleOutcomesAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return ValueTask.FromResult(GetPossibleOutcomes(workflowContext, activityContext));
    }

    public virtual IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return [];
    }

    public virtual Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Task.FromResult(CanExecute(workflowContext, activityContext));
    }

    public virtual bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return true;
    }

    public virtual Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Task.FromResult(Execute(workflowContext, activityContext));
    }

    public virtual ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Noop();
    }

    public virtual Task<ActivityExecutionResult> ResumeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Task.FromResult(Resume(workflowContext, activityContext));
    }

    public virtual ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Noop();
    }

    public virtual Task OnInputReceivedAsync(WorkflowExecutionContext workflowContext, IDictionary<string, object> input)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowStartingAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowStartedAsync(WorkflowExecutionContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowRestartingAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowRestartedAsync(WorkflowExecutionContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowResumingAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnWorkflowResumedAsync(WorkflowExecutionContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnActivityExecutingAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnActivityExecutedAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Task.CompletedTask;
    }

    protected static IEnumerable<Outcome> Outcome(params LocalizedString[] names)
        => names.Select(s => new Outcome(s));

    protected static ActivityExecutionResult Outcome(params string[] names)
        => new ActivityExecutionResult(names);

    protected static ActivityExecutionResult Outcome(params IEnumerable<string> names)
        => new ActivityExecutionResult(names);

    [Obsolete("This methods is deprecated. Use Outcome(params LocalizedString[] names) instead.")]
    protected static IEnumerable<Outcome> Outcomes(params LocalizedString[] names) => Outcome(names);

    [Obsolete("This methods is deprecated. Use Outcome(params LocalizedString[] names) instead.")]
    protected static IEnumerable<Outcome> Outcomes(IEnumerable<LocalizedString> names) => Outcome(names.ToArray());

    [Obsolete("This methods is deprecated. Use Outcome(params string[] names) instead.")]
    protected static ActivityExecutionResult Outcomes(string name) => Outcome(name);

    [Obsolete("This methods is deprecated. Use Outcome(params string[] names) instead.")]
    protected static ActivityExecutionResult Outcomes(params string[] names) => Outcome(names);

    [Obsolete("This methods is deprecated. Use Outcome(params string[] names) instead.")]
    protected static ActivityExecutionResult Outcomes(IEnumerable<string> names) => Outcome(names.ToArray());

    protected static ActivityExecutionResult Halt()
    {
        return ActivityExecutionResult.Halted;
    }

    protected static ActivityExecutionResult Noop()
    {
        return ActivityExecutionResult.Empty;
    }

    protected virtual T GetProperty<T>(Func<T> defaultValue = null, [CallerMemberName] string name = null)
    {
        var item = Properties[name];
        return item != null ? item.ToObject<T>() : defaultValue != null ? defaultValue() : default;
    }

    protected virtual T GetProperty<T>(Type type, Func<T> defaultValue = null, [CallerMemberName] string name = null)
    {
        var item = Properties[name];
        return item != null ? (T)item.ToObject(type) : defaultValue != null ? defaultValue() : default;
    }

    protected virtual void SetProperty(object value, [CallerMemberName] string name = null)
    {
        // Properties[name] = JToken.FromObject(value);
        Properties[name] = value is JsonNode node ? node.DeepClone() : JNode.FromObject(value);
    }
}
