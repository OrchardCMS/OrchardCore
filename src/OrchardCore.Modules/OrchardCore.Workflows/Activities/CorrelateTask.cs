using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class CorrelateTask : TaskActivity
    {
        public CorrelateTask(IStringLocalizer<CorrelateTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(CorrelateTask);
        public override LocalizedString Category => T["Primitives"];
        public override LocalizedString Description => T["Correlates the current workflow instance with a configurable value."];

        public WorkflowExpression<string> Expression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var value = workflowContext.Evaluate(Expression);
            workflowContext.CorrelationId = value;

            yield return "Done";
        }
    }
}