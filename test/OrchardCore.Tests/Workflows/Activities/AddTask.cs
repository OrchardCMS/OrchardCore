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
        public override LocalizedString Description => T["Adds two numbers and pushes the result onto the stack."];

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

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var a = await workflowContext.EvaluateAsync(A);
            var b = await workflowContext.EvaluateAsync(B);
            var result = a + b;

            workflowContext.Stack.Push(result);
            return new[] { "Done" };
        }
    }
}
