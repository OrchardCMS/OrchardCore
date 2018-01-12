using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class ForLoopTask : TaskActivity
    {
        public ForLoopTask(IStringLocalizer<ForLoopTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(ForLoopTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Iterates over a branch of execution for N times."];

        /// <summary>
        /// An expression evaluating to the number of times to loop.
        /// </summary>
        public WorkflowExpression<int> Count
        {
            get => GetProperty(() => new WorkflowExpression<int>());
            set => SetProperty(value);
        }

        /// <summary>
        /// The current number of iterations executed.
        /// </summary>
        public int Index
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Iterate"], T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var count = await workflowContext.EvaluateScriptAsync(Count);

            if (Index < count)
            {
                workflowContext.LastResult = Index;
                Index++;
                return Outcomes("Iterate");
            }
            else
            {
                return Outcomes("Done");
            }
        }
    }
}