using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        private readonly IScriptingManager _scriptingManager;
        private readonly IEnumerable<IWorkflowContextHandler> _handlers;
        private readonly ILogger<WorkflowContext> _logger;

        public WorkflowContext
        (
            WorkflowDefinitionRecord workflowDefinitionRecord,
            WorkflowInstanceRecord workflowInstanceRecord,
            IEnumerable<ActivityContext> activities,
            IEnumerable<IWorkflowContextHandler> handlers,
            IScriptingManager scriptingManager,
            ILogger<WorkflowContext> logger
        )
        {
            _scriptingManager = scriptingManager;
            _handlers = handlers;
            _logger = logger;

            WorkflowDefinition = workflowDefinitionRecord;
            WorkflowInstance = workflowInstanceRecord;
            Activities = activities.ToList();
            State = workflowInstanceRecord.State.ToObject<WorkflowState>();
        }

        public WorkflowDefinitionRecord WorkflowDefinition { get; }
        public WorkflowInstanceRecord WorkflowInstance { get; }
        public IList<ActivityContext> Activities { get; }
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

        public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            return (T)await EvaluateScriptAsync(expression.Expression, scopedMethodProviders);
        }

        public Task EvaluateAsync(string script, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            return EvaluateScriptAsync(script, scopedMethodProviders);
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

        private async Task<object> EvaluateScriptAsync(string script, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            var prefix = !String.IsNullOrWhiteSpace(WorkflowDefinition.ScriptingEngine) ? WorkflowDefinition.ScriptingEngine : "js";
            var directive = $"{prefix}:{script}";
            var context = new WorkflowContextScriptEvalContext(this);

            context.ScopedMethodProviders.AddRange(scopedMethodProviders);

            await _handlers.InvokeAsync(async x => await x.EvaluatingScriptAsync(context), _logger);
            return _scriptingManager.Evaluate(directive, context.ScopedMethodProviders);
        }
    }
}