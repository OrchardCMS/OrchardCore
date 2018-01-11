using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

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
        public override LocalizedString Description => T["Correlates the current workflow instance with a value."];

        public WorkflowExpression<string> Value
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var value = (await workflowContext.EvaluateExpressionAsync(Value))?.Trim();
            workflowContext.CorrelationId = value;

            return Outcomes("Done");
        }
    }
}