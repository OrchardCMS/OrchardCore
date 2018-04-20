using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Activities
{
    public class SignalEvent : EventActivity
    {
        public static string EventName => nameof(SignalEvent);
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public SignalEvent(IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizer<SignalEvent> localizer)
        {
            _expressionEvaluator = expressionEvaluator;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => EventName;
        public override LocalizedString Category => T["HTTP"];

        public WorkflowExpression<string> SignalName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var signalName = await _expressionEvaluator.EvaluateAsync(SignalName, workflowContext);
            return string.Equals(workflowContext.Input.GetValue<string>("Signal"), signalName, StringComparison.OrdinalIgnoreCase);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }
    }
}