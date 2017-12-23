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

namespace OrchardCore.Workflows.Services
{
    public abstract class Activity : Entity, IActivity
    {
        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public abstract IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext);

        public virtual bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public virtual Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Task.FromResult(Execute(workflowContext, activityContext));
        }

        public virtual IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield break;
        }

        public virtual void OnWorkflowStarting(WorkflowContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
        }

        public virtual void OnWorkflowStarted(WorkflowContext context)
        {
        }

        public virtual void OnWorkflowResuming(WorkflowContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
        }

        public virtual void OnWorkflowResumed(WorkflowContext context)
        {
        }

        public virtual void OnActivityExecuting(WorkflowContext workflowContext, ActivityContext activityContext, CancellationToken cancellationToken = default(CancellationToken))
        {
        }

        public virtual void OnActivityExecuted(WorkflowContext workflowContext, ActivityContext activityContext)
        {
        }

        protected Outcome Outcome(LocalizedString name)
        {
            return new Outcome(name);
        }

        protected IEnumerable<Outcome> Outcomes(params LocalizedString[] names)
        {
            return names.Select(x => new Outcome(x));
        }

        protected virtual T GetProperty<T>([CallerMemberName]string name = null)
        {
            var item = Properties[name];
            return item != null ? item.ToObject<T>() : default(T);
        }

        protected virtual void SetProperty<T>(T value, [CallerMemberName]string name = null)
        {
            Properties[name] = JToken.FromObject(value);
        }
    }
}