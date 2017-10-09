using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class Activity : IActivity
    {
        public abstract string Name { get; }
        public abstract LocalizedString Category { get; }
        public abstract LocalizedString Description { get; }

        public virtual string Form
        {
            get { return null; }
        }

        public abstract IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext);

        public virtual bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }

        public abstract Task<IEnumerable<LocalizedString>> Execute(WorkflowContext workflowContext, ActivityContext activityContext);

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
    }
}