using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class JoinTask : TaskActivity
    {
        protected readonly IStringLocalizer S;

        public enum JoinMode
        {
            WaitAll,
            WaitAny
        }

        public JoinTask(IStringLocalizer<JoinTask> localizer)
        {
            S = localizer;
        }

        public override string Name => nameof(JoinTask);

        public override LocalizedString DisplayText => S["Join Task"];

        public override LocalizedString Category => S["Control Flow"];

        public JoinMode Mode
        {
            get => GetProperty(() => JoinMode.WaitAll);
            set => SetProperty(value);
        }

        private IList<string> Branches
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Joined"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var branches = Branches;
            var inboundTransitions = workflowContext.GetInboundTransitions(activityContext.ActivityRecord.ActivityId);
            var done = false;

            switch (Mode)
            {
                case JoinMode.WaitAll:
                    done = inboundTransitions.All(x => branches.Contains(GetTransitionKey(x)));
                    break;
                case JoinMode.WaitAny:
                    done = inboundTransitions.Any(x => branches.Contains(GetTransitionKey(x)));

                    if (done)
                    {
                        // Remove any inbound blocking activities.
                        var ancestorActivityIds = workflowContext.GetInboundActivityPath(activityContext.ActivityRecord.ActivityId).ToList();
                        var blockingActivities = workflowContext.Workflow.BlockingActivities.Where(x => ancestorActivityIds.Contains(x.ActivityId)).ToList();

                        foreach (var blockingActivity in blockingActivities)
                        {
                            workflowContext.Workflow.BlockingActivities.Remove(blockingActivity);
                        }
                    }
                    break;
            }

            if (done)
            {
                return Outcomes("Joined");
            }

            return Noop();
        }
        public override Task OnActivityExecutedAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            // Get outbound transitions of the executing activity.
            var outboundTransitions = workflowContext.GetOutboundTransitions(activityContext.ActivityRecord.ActivityId);

            // Get any transition that is pointing to this activity.
            var inboundTransitionsQuery =
                from transition in outboundTransitions
                let destinationActivity = workflowContext.GetActivity(transition.DestinationActivityId)
                where destinationActivity.Activity.Name == Name
                select transition;

            var inboundTransitions = inboundTransitionsQuery.ToList();

            foreach (var inboundTransition in inboundTransitions)
            {
                var mergeActivity = (JoinTask)workflowContext.GetActivity(inboundTransition.DestinationActivityId).Activity;
                var branches = mergeActivity.Branches;
                mergeActivity.Branches = branches.Union(new[] { GetTransitionKey(inboundTransition) }).Distinct().ToList();
            }

            return Task.CompletedTask;
        }

        private static string GetTransitionKey(Transition transition)
        {
            var sourceActivityId = transition.SourceActivityId;
            var sourceOutcomeName = transition.SourceOutcomeName;

            return $"@{sourceActivityId}_{sourceOutcomeName}";
        }
    }
}
