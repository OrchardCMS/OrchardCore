using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Workflows.Activities
{
    public class AddTask : TaskActivity
    {
        public AddTask(IStringLocalizer t)
        {
            T = t;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(AddTask);
        public override LocalizedString Category => T["Test"];

        public WorkflowExpression<double> A
        {
            get => GetProperty<WorkflowExpression<double>>();
            set => SetProperty(value);
        }

        public WorkflowExpression<double> B
        {
            get => GetProperty<WorkflowExpression<double>>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var a = await workflowContext.EvaluateScriptAsync(A);
            var b = await workflowContext.EvaluateScriptAsync(B);
            var result = a + b;

            workflowContext.LastResult = result;
            return Outcomes("Done");
        }
    }
}
