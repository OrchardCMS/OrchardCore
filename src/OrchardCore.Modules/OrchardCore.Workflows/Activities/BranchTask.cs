using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class BranchTask : TaskActivity
    {
        public BranchTask(IStringLocalizer<BranchTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(BranchTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Splits workflow execution into branches."];

        public IList<string> Branches
        {
            get => GetProperty(() => new List<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Branches.Select(x => Outcome(T[x]));
        }

        public override ActivityExecutionResult Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(Branches);
        }
    }
}