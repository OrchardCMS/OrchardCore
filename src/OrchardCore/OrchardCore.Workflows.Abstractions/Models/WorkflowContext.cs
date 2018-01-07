using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        public WorkflowContext
        (
            WorkflowDefinitionRecord workflowDefinitionRecord,
            WorkflowInstanceRecord workflowInstanceRecord,
            IEnumerable<ActivityContext> activities,
            IScriptingManager scriptingManager,
            IServiceProvider serviceProvider,
        )
        {
            WorkflowDefinition = workflowDefinitionRecord;
            WorkflowInstance = workflowInstanceRecord;
            Activities = activities.ToList();
            ScriptingManager = scriptingManager;
            State = workflowInstanceRecord.State.ToObject<WorkflowState>();
        }

        protected IEnumerable<IWorkflowContextProvider> WorkflowContextProviders { get; set; }
        public WorkflowDefinitionRecord WorkflowDefinition { get; }
        public WorkflowInstanceRecord WorkflowInstance { get; }
        public IList<ActivityContext> Activities { get; }
        public IScriptingManager ScriptingManager { get; }
        public WorkflowState State { get; }
        public string CorrelationId
        {
            get => WorkflowInstance.CorrelationId;
            set => WorkflowInstance.CorrelationId = value;
        }
        public Stack<object> Stack => State.Stack;
        public IDictionary<string, object> Input => State.Input;
        public IDictionary<string, object> Output => State.Output;
        public IDictionary<string, object> Variables => State.Variables;
        public WorkflowStatus Status
        {
            get => WorkflowInstance.Status;
            set => WorkflowInstance.Status = value;
        }

        public ActivityContext GetActivity(int activityId)
        {
            return Activities.Single(x => x.ActivityRecord.Id == activityId);
        }

        public virtual T Evaluate<T>(WorkflowExpression<T> expression, IEnumerable<IGlobalMethodProvider> scopedMethodProviders = null)
        {
            var prefix = !String.IsNullOrWhiteSpace(WorkflowDefinition.ScriptingEngine) ? WorkflowDefinition.ScriptingEngine : "js";
            var directive = $"{prefix}:{expression}";
            return (T)ScriptingManager.Evaluate(directive, scopedMethodProviders);
        }

        public virtual void Evaluate(string script, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            ScriptingManager.GlobalMethodProviders.AddRange(scopedMethodProviders);
            ScriptingManager.Evaluate(script);
            ScriptingManager.GlobalMethodProviders.RemoveRange(scopedMethodProviders);
        }
        public void Fault(Exception exception, ActivityContext activityContext)
        {
            WorkflowInstance.Status = WorkflowStatus.Faulted;
            WorkflowInstance.FaultMessage = exception.Message;
        }

        public IEnumerable<TransitionRecord> GetInboundTransitions(ActivityRecord activityRecord)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord, LocalizedString outcome)
        {
            throw new NotImplementedException();
        }
    }
}