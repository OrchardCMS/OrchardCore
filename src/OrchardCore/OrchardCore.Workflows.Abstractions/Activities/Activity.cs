using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public abstract class Activity : Entity, IActivity
    {
        public abstract string Name { get; }
        public abstract LocalizedString DisplayText { get; }
        public abstract LocalizedString Category { get; }
        public virtual bool HasEditor => true;

        public abstract IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext);

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

        protected static Outcome Outcome(LocalizedString name)
        {
            return new Outcome(name);
        }

        protected static IEnumerable<Outcome> Outcomes(params LocalizedString[] names)
        {
            return names.Select(x => new Outcome(x));
        }

        protected static IEnumerable<Outcome> Outcomes(IEnumerable<LocalizedString> names)
        {
            return names.Select(x => new Outcome(x));
        }

        protected static ActivityExecutionResult Outcomes(string name)
        {
            return new ActivityExecutionResult(new string[] { name });
        }

        protected static ActivityExecutionResult Outcomes(params string[] names)
        {
            return new ActivityExecutionResult(names);
        }

        protected static ActivityExecutionResult Outcomes(IEnumerable<string> names)
        {
            return new ActivityExecutionResult(names);
        }

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
            Properties[name] = JToken.FromObject(value);
        }
    }
}
