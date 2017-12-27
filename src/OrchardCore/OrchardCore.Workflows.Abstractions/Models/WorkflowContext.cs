using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        private readonly IScriptingManager _scriptingManager;

        public WorkflowContext
        (
            WorkflowDefinitionRecord workflowDefinitionRecord,
            WorkflowInstanceRecord workflowInstanceRecord,
            IEnumerable<ActivityContext> activities,
            IScriptingManager scriptingManager
        )
        {
            _scriptingManager = scriptingManager;

            WorkflowDefinition = workflowDefinitionRecord;
            WorkflowInstance = workflowInstanceRecord;
            Activities = activities.ToList();
            State = workflowInstanceRecord.State.ToObject<WorkflowState>();
        }

        public WorkflowDefinitionRecord WorkflowDefinition { get; }
        public WorkflowInstanceRecord WorkflowInstance { get; }
        public IList<ActivityContext> Activities { get; }
        public WorkflowState State { get; }
        public Stack<object> Stack => State.Stack;
        public IDictionary<string, object> Input => State.Input;
        public IDictionary<string, object> Output => State.Output;
        public IDictionary<string, object> Variables => State.Variables;

        public ActivityContext GetActivity(int activityId)
        {
            return Activities.Single(x => x.ActivityRecord.Id == activityId);
        }

        public T Evaluate<T>(WorkflowExpression<T> expression)
        {
            return (T)_scriptingManager.Evaluate(expression.Expression);
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