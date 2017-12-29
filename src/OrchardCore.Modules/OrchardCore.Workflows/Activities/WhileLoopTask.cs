using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class WhileLoopTask : TaskActivity
    {
        public WhileLoopTask(IStringLocalizer<WhileLoopTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(WhileLoopTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Iterates over a branch of execution while the specified condition is true."];

        /// <summary>
        /// An expression evaluating to the number of times to loop.
        /// </summary>
        public WorkflowExpression<bool> ConditionExpression
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Iterate"], T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var loop = workflowContext.Evaluate(ConditionExpression);

            if (loop)
            {
                yield return "Iterate";
            }
            else
            {
                yield return "Done";
            }
        }
    }
}