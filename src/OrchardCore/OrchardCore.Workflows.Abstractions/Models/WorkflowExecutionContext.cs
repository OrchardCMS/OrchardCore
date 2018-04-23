using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowExecutionContext
    {
        private readonly IEnumerable<IWorkflowExecutionContextHandler> _handlers;
        private readonly ILogger<WorkflowExecutionContext> _logger;

        public WorkflowExecutionContext
        (
            WorkflowType workflowTypeRecord,
            Workflow workflowRecord,
            IDictionary<string, object> input,
            IDictionary<string, object> output,
            IDictionary<string, object> properties,
            IList<ExecutedActivity> executedActivities,
            object lastResult,
            IEnumerable<ActivityContext> activities,
            IEnumerable<IWorkflowExecutionContextHandler> handlers,
            ILogger<WorkflowExecutionContext> logger
        )
        {
            _handlers = handlers;
            _logger = logger;

            Input = input ?? new Dictionary<string, object>();
            Output = output ?? new Dictionary<string, object>();
            Properties = properties ?? new Dictionary<string, object>();
            ExecutedActivities = new Stack<ExecutedActivity>(executedActivities ?? new List<ExecutedActivity>());
            LastResult = lastResult;
            WorkflowTypeRecord = workflowTypeRecord;
            WorkflowRecord = workflowRecord;
            Activities = activities.ToDictionary(x => x.ActivityRecord.ActivityId);
        }

        public Workflow WorkflowRecord { get; }
        public WorkflowType WorkflowTypeRecord { get; }
        public IDictionary<string, ActivityContext> Activities { get; }

        public string WorkflowInstanceId
        {
            get => WorkflowRecord.WorkflowId;
        }

        public string CorrelationId
        {
            get => WorkflowRecord.CorrelationId;
            set => WorkflowRecord.CorrelationId = value;
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
            get => WorkflowRecord.Status;
            set => WorkflowRecord.Status = value;
        }

        /// <summary>
        /// Keeps track of which activities executed in which order.
        /// </summary>
        public Stack<ExecutedActivity> ExecutedActivities { get; set; }

        public ActivityContext GetActivity(string activityId)
        {
            return Activities[activityId];
        }
        
        public void Fault(Exception exception, ActivityContext activityContext)
        {
            WorkflowRecord.Status = WorkflowStatus.Faulted;
            WorkflowRecord.FaultMessage = exception.Message;
        }

        public IEnumerable<Transition> GetInboundTransitions(string activityId)
        {
            return WorkflowTypeRecord.Transitions.Where(x => x.DestinationActivityId == activityId).ToList();
        }

        public IEnumerable<Transition> GetOutboundTransitions(string activityId)
        {
            return WorkflowTypeRecord.Transitions.Where(x => x.SourceActivityId == activityId).ToList();
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