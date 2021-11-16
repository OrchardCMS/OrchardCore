using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class CorrelateTask : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly IStringLocalizer S;

        public CorrelateTask(IStringLocalizer<CorrelateTask> localizer, IWorkflowExpressionEvaluator expressionEvaluator)
        {
            S = localizer;
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(CorrelateTask);

        public override LocalizedString DisplayText => S["Correlate Task"];

        public override LocalizedString Category => S["Primitives"];

        public WorkflowExpression<string> Value
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var value = (await _expressionEvaluator.EvaluateAsync(Value, workflowContext, null))?.Trim();
            workflowContext.CorrelationId = value;

            return Outcomes("Done");
        }
    }
}
