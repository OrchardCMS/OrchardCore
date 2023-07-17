using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class ForkTask : TaskActivity
    {
        protected readonly IStringLocalizer S;

        public ForkTask(IStringLocalizer<ForkTask> localizer)
        {
            S = localizer;
        }

        public override string Name => nameof(ForkTask);

        public override LocalizedString DisplayText => S["Fork Task"];

        public override LocalizedString Category => S["Control Flow"];

        public IList<string> Forks
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Forks.Select(x => Outcome(S[x]));
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(Forks);
        }
    }
}
