using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

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

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var a = workflowContext.Evaluate(A);
            var b = workflowContext.Evaluate(B);
            var result = a + b;

            workflowContext.Stack.Push(result);
            yield return "Done";
        }
    }
}
