using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        private readonly IEnumerable<IWorkflowContextHandler> _handlers;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowContext> _logger;

        public WorkflowContext
        (
            WorkflowDefinitionRecord workflowDefinitionRecord,
            WorkflowInstanceRecord workflowInstanceRecord,
            IServiceProvider serviceProvider,
            IDictionary<string, object> input,
            IEnumerable<ActivityContext> activities,
            IEnumerable<IWorkflowContextHandler> handlers,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            ILogger<WorkflowContext> logger
        )
        {
            _serviceProvider = serviceProvider;
            _handlers = handlers;
            _expressionEvaluator = expressionEvaluator;
            _scriptEvaluator = scriptEvaluator;
            _logger = logger;

            Input = input ?? new Dictionary<string, object>();
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

        /// <summary>
        /// A dictionary of non-serialized values provided by the initiator of the workflow.
        /// </summary>
        public IDictionary<string, object> Input { get; }

        /// <summary>
        /// A dictionary of non-serialized values provided to the initiator of the workflow.
        /// </summary>
        public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A dictionary of serialized values provided by the workflow activities.
        /// </summary>
        public IDictionary<string, object> Properties => State.Properties;

        /// <summary>
        /// The value returned from the previous activity, if any.
        /// </summary>
        public Stack<object> Stack => State.Stack;

        public WorkflowStatus Status
        {
            get => WorkflowInstance.Status;
            set => WorkflowInstance.Status = value;
        }

        public ActivityContext GetActivity(int activityId)
        {
            return Activities.Single(x => x.ActivityRecord.Id == activityId);
        }

        public Task<T> EvaluateExpressionAsync<T>(WorkflowExpression<T> expression)
        {
            return _expressionEvaluator.EvaluateAsync(expression, this);
        }

        public Task<T> EvaluateScriptAsync<T>(WorkflowExpression<T> expression, params IGlobalMethodProvider[] scopedMethodProviders)
        {
            return _scriptEvaluator.EvaluateAsync(expression, this, scopedMethodProviders);
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