using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class LogTask : TaskActivity
    {
        private readonly ILogger _logger;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        protected readonly IStringLocalizer S;

        public LogTask(ILogger<LogTask> logger, IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizer<LogTask> localizer)
        {
            _logger = logger;
            _expressionEvaluator = expressionEvaluator;
            S = localizer;
        }
        public override string Name => nameof(LogTask);

        public override LocalizedString DisplayText => S["Log Task"];

        public override LocalizedString Category => S["Primitives"];

        public LogLevel LogLevel
        {
            get => GetProperty(() => LogLevel.Information);
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Text
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
            var text = await _expressionEvaluator.EvaluateAsync(Text, workflowContext, null);
            var logLevel = LogLevel;

            _logger.Log(logLevel, 0, text, null, (state, error) => state.ToString());
            return Outcomes("Done");
        }
    }
}
