using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowExecutionContext
    {
        public WorkflowExecutionContext
        (
            WorkflowType workflowType,
            Workflow workflow,
            IDictionary<string, object> input,
            IDictionary<string, object> output,
            IDictionary<string, object> properties,
            IList<ExecutedActivity> executedActivities,
            object lastResult,
            IEnumerable<ActivityContext> activities
        )
        {
            Input = input ?? new Dictionary<string, object>();
            Output = output ?? new Dictionary<string, object>();
            Properties = properties ?? new Dictionary<string, object>();
            ExecutedActivities = new Stack<ExecutedActivity>(executedActivities ?? new List<ExecutedActivity>());
            LastResult = lastResult;
            WorkflowType = workflowType;
            Workflow = workflow;
            Activities = activities.ToDictionary(x => x.ActivityRecord.ActivityId);
        }

        public Workflow Workflow { get; }
        public WorkflowType WorkflowType { get; }
        public IDictionary<string, ActivityContext> Activities { get; }

        public string WorkflowId
        {
            get => Workflow.WorkflowId;
        }

        public string CorrelationId
        {
            get => Workflow.CorrelationId;
            set => Workflow.CorrelationId = value;
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
            get => Workflow.Status;
            set => Workflow.Status = value;
        }

        /// <summary>
        /// Keeps track of which activities executed in which order.
        /// </summary>
        public Stack<ExecutedActivity> ExecutedActivities { get; set; }

        public ActivityContext GetActivity(string activityId)
        {
            return Activities[activityId];
        }

        public void Fault(Exception exception, ActivityContext _)
        {
            Workflow.Status = WorkflowStatus.Faulted;
            Workflow.FaultMessage = exception.Message;
        }

        public IEnumerable<Transition> GetInboundTransitions(string activityId)
        {
            return WorkflowType.Transitions.Where(x => x.DestinationActivityId == activityId).ToList();
        }

        public IEnumerable<Transition> GetOutboundTransitions(string activityId)
        {
            return WorkflowType.Transitions.Where(x => x.SourceActivityId == activityId).ToList();
        }

        /// <summary>
        /// Returns the full path of incoming activities.
        /// </summary>
        public IEnumerable<string> GetInboundActivityPath(string activityId)
        {
            return GetInboundActivityPathInternal(activityId, activityId).Distinct().ToList();
        }

        private IEnumerable<string> GetInboundActivityPathInternal(string activityId, string startingPointActivityId)
        {
            foreach (var transition in GetInboundTransitions(activityId))
            {
                // Circuit breaker: Detect workflows that implement repeating flows to prevent an infinite loop here.
                if (transition.SourceActivityId == startingPointActivityId)
                {
                    yield break;
                }
                else
                {
                    yield return transition.SourceActivityId;

                    foreach (var parentActivityId in GetInboundActivityPathInternal(transition.SourceActivityId, startingPointActivityId).Distinct())
                    {
                        yield return parentActivityId;
                    }
                }
            }
        }
    }
}
