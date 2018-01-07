using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

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

        public WorkflowExpression<string> SignalNameExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<bool> ConditionExpression
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var signalName = await workflowContext.EvaluateAsync(SignalNameExpression);
            var conditionResult = await workflowContext.EvaluateAsync(ConditionExpression);
            return workflowContext.Input.GetValue<string>("Signal") == signalName;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return "Done";
        }
    }
}