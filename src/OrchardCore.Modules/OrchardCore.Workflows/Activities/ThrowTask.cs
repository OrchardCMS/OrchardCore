using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class ThrowTask : TaskActivity
    {
        private readonly ILogger<ThrowTask> _logger;

        public ThrowTask(ILogger<ThrowTask> logger, IStringLocalizer<ThrowTask> localizer)
        {
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(ThrowTask);
        public override LocalizedString Category => T["Primitives"];

        public string ExceptionType
        {
            get => GetProperty(() => "System.Exception");
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Message
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            yield break;
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var exceptionType = Type.GetType(ExceptionType);
            var message = await workflowContext.EvaluateExpressionAsync(Message);
            var hasMessage = string.IsNullOrWhiteSpace(message);
            var exception = (Exception)(hasMessage ? Activator.CreateInstance(exceptionType, message) : Activator.CreateInstance(exceptionType));

            _logger.LogError(exception, message);
            throw exception;
        }
    }
}