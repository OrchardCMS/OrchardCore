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
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        protected readonly IStringLocalizer S;

        public CorrelateTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<CorrelateTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            S = localizer;
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
            var value = (await _scriptEvaluator.EvaluateAsync(Value, workflowContext))?.Trim();
            workflowContext.CorrelationId = value;

            return Outcomes("Done");
        }
    }
}
