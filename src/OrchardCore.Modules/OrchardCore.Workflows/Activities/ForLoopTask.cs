using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class ForLoopTask : Activity
    {
        public ForLoopTask(IStringLocalizer<EvaluateExpressionTask> localizer)
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
        public WorkflowExpression<int> CountExpression
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

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var count = workflowContext.Evaluate(CountExpression);
            var index = Index;

            if (index < count)
            {
                Index++;
                yield return "Iterate";
            }
            else
                yield return "Done";
        }
    }
}