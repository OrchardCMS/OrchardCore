using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class LogTask : TaskActivity
    {
        private readonly ILogger<LogTask> _logger;

        public LogTask(ILogger<LogTask> logger, IStringLocalizer<NotifyTask> localizer)
        {
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(LogTask);
        public override LocalizedString Category => T["Primitives"];

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
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var text = await workflowContext.EvaluateExpressionAsync(Text);
            var logLevel = LogLevel;

            _logger.Log(logLevel, 0, text, null, (state, error) => state.ToString());
            return Outcomes("Done");
        }
    }
}