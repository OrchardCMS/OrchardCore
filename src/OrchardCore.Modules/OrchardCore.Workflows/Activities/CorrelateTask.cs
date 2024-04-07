using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class CorrelateTask : TaskActivity<CorrelateTask>
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;

        public CorrelateTask(IWorkflowScriptEvaluator scriptEvaluator, IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizer<CorrelateTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }

        public override LocalizedString DisplayText => S["Correlate Task"];

        public override LocalizedString Category => S["Primitives"];

        public WorkflowExpression<string> Value
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public string Syntax
        {
            get => GetProperty(() => "JavaScript");
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var value = Syntax switch {
                "Liquid" => await _expressionEvaluator.EvaluateAsync(Value, workflowContext, null),
                "JavaScript" => (await _scriptEvaluator.EvaluateAsync(Value, workflowContext, null))?.Trim(),
                _ => null 
            };

            workflowContext.CorrelationId = value;

            return Outcomes("Done");
        }
    }
}
