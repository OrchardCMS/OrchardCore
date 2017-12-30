using System.Collections.Generic;
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

        public string SignalName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<bool> ConditionExpression
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var conditionResult = workflowContext.Evaluate(ConditionExpression);
            return workflowContext.Input.GetValue<string>("Signal") == SignalName;
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