using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class MergeTask : TaskActivity
    {
        public MergeTask(IStringLocalizer<MergeTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(MergeTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Merges workflow execution back into a single branch."];
        public override bool HasEditor => false;

        private IList<string> Branches
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Merged"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // Wait for all incoming branches to have executed their activity.
            var branches = Branches;
            var inboundActivities = workflowContext.GetInboundTransitions(activityContext.ActivityRecord.Id);
            var done = inboundActivities.All(x => branches.Contains(GetTransitionKey(x)));

            if (done)
            {
                return Outcomes("Merged");
            }

            return Noop();
        }
        public override Task OnActivityExecutedAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // Get outbound transitions of the executing activity.
            var outboundTransitions = workflowContext.GetOutboundTransitions(activityContext.ActivityRecord.Id);

            // Get any transition that is pointing to this activity.
            var inboundTransitionsQuery =
                from transition in outboundTransitions
                let destinationActivity = workflowContext.GetActivity(transition.DestinationActivityId)
                where destinationActivity.Activity.Name == Name
                select transition;

            var inboundTransitions = inboundTransitionsQuery.ToList();

            foreach (var inboundTransition in inboundTransitions)
            {
                var mergeActivity = (MergeTask)workflowContext.GetActivity(inboundTransition.DestinationActivityId).Activity;
                var branches = mergeActivity.Branches;
                mergeActivity.Branches = branches.Union(new[] { GetTransitionKey(inboundTransition) }).Distinct().ToList();
            }

            return Task.CompletedTask;
        }

        private string GetTransitionKey(TransitionRecord transition)
        {
            var sourceActivityId = transition.SourceActivityId;
            var sourceOutcomeName = transition.SourceOutcomeName;

            return $"@{sourceActivityId}_{sourceOutcomeName}";
        }
    }
}