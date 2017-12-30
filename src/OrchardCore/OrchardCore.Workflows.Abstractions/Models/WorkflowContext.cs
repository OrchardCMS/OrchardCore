using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Scripting;
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
            IScriptingManager scriptingManager
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
        public string CorrelationId => WorkflowInstance.CorrelationId;
        public Stack<object> Stack => State.Stack;
        public IDictionary<string, object> Input => State.Input;
        public IDictionary<string, object> Output => State.Output;
        public IDictionary<string, object> Variables => State.Variables;

        public ActivityContext GetActivity(int activityId)
        {
            return Activities.Single(x => x.ActivityRecord.Id == activityId);
        }

        public virtual T Evaluate<T>(WorkflowExpression<T> expression)
        {
            return (T)ScriptingManager.Evaluate(expression.Expression);
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