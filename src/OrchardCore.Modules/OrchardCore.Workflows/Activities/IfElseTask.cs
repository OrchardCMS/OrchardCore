using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class IfElseTask : TaskActivity
    {
        public IfElseTask(IStringLocalizer<IfElseTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(IfElseTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Evaluates the specified condition and continues execution based on the outcome."];

        /// <summary>
        /// A script evaluating to either true or false.
        /// </summary>
        public WorkflowExpression<bool> Condition
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["True"], T["False"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var result = await workflowContext.EvaluateScriptAsync(Condition);
            return Outcomes(result ? "True" : "False");
        }
    }
}