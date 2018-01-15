using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class SignalEvent : EventActivity
    {
        public static string EventName => nameof(SignalEvent);

        public SignalEvent(IStringLocalizer<SignalEvent> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => EventName;
        public override LocalizedString Category => T["Events"];
        public override LocalizedString Description => T["Executes when the specified signal name is triggered."];

        public WorkflowExpression<string> SignalName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var signalName = await workflowContext.EvaluateExpressionAsync(SignalName);
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