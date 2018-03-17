using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowExecutionContext
    {
        private readonly IEnumerable<IWorkflowExecutionContextHandler> _handlers;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly ILogger<WorkflowExecutionContext> _logger;

        public WorkflowExecutionContext
        (
            WorkflowDefinition workflowDefinitionRecord,
            WorkflowInstance workflowInstanceRecord,
            IDictionary<string, object> input,
            IDictionary<string, object> output,
            IDictionary<string, object> properties,
            IList<ExecutedActivity> executedActivities,
            object lastResult,
            IEnumerable<ActivityContext> activities,
            IEnumerable<IWorkflowExecutionContextHandler> handlers,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            ILogger<WorkflowExecutionContext> logger
        )
        {
            _handlers = handlers;
            _expressionEvaluator = expressionEvaluator;
            _scriptEvaluator = scriptEvaluator;
            _logger = logger;

            Input = input ?? new Dictionary<string, object>();
            Output = output ?? new Dictionary<string, object>();
            Properties = properties ?? new Dictionary<string, object>();
            ExecutedActivities = new Stack<ExecutedActivity>(executedActivities ?? new List<ExecutedActivity>());
            LastResult = lastResult;
            WorkflowDefinitionRecord = workflowDefinitionRecord;
            WorkflowInstanceRecord = workflowInstanceRecord;
            Activities = activities.ToDictionary(x => x.ActivityRecord.ActivityId);
        }

        public WorkflowInstance WorkflowInstanceRecord { get; }
        public WorkflowDefinition WorkflowDefinitionRecord { get; }
        public IDictionary<string, ActivityContext> Activities { get; }

        public string WorkflowInstanceId
        {
            get => WorkflowInstanceRecord.WorkflowInstanceId;
        }

        public string CorrelationId
        {
            get => WorkflowInstanceRecord.CorrelationId;
            set => WorkflowInstanceRecord.CorrelationId = value;
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
            get => WorkflowInstanceRecord.Status;
            set => WorkflowInstanceRecord.Status = value;
        }

        /// <summary>
        /// Keeps track of which activities executed in which order.
        /// </summary>
        public Stack<ExecutedActivity> ExecutedActivities { get; set; }

        public ActivityContext GetActivity(string activityId)
        {
            return Activities[activityId];
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
            WorkflowInstanceRecord.Status = WorkflowStatus.Faulted;
            WorkflowInstanceRecord.FaultMessage = exception.Message;
        }

        public IEnumerable<Transition> GetInboundTransitions(string activityId)
        {
            return WorkflowDefinitionRecord.Transitions.Where(x => x.DestinationActivityId == activityId).ToList();
        }

        public IEnumerable<Transition> GetOutboundTransitions(string activityId)
        {
            return WorkflowDefinitionRecord.Transitions.Where(x => x.SourceActivityId == activityId).ToList();
        }

        /// <summary>
        /// Returns the full path of incoming activities.
        /// </summary>
        public IEnumerable<string> GetInboundActivityPath(string activityId)
        {
            return GetInboundActivityPathInternal(activityId).Distinct().ToList();
        }

        private IEnumerable<string> GetInboundActivityPathInternal(string activityId)
        {
            foreach (var transition in GetInboundTransitions(activityId))
            {
                yield return transition.SourceActivityId;

                foreach (var parentActivityId in GetInboundActivityPath(transition.SourceActivityId))
                {
                    yield return parentActivityId;
                }
            }
        }
    }
}