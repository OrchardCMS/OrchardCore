using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class EvaluateExpressionTask : TaskActivity
    {
        public EvaluateExpressionTask(IStringLocalizer<EvaluateExpressionTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(EvaluateExpressionTask);
        public override LocalizedString Category => T["Primitives"];
        public override LocalizedString Description => T["Evaluates an expression and pushes the result onto the stack."];

        public WorkflowExpression<object> Expression
        {
            get => GetProperty(() => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var value = await workflowContext.EvaluateAsync(Expression);
            workflowContext.Stack.Push(value);

            return Outcomes("Done");
        }
    }
}