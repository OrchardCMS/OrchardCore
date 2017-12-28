using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class SetOutputTask : Activity
    {
        public SetOutputTask(IStringLocalizer<SetOutputTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(SetOutputTask);
        public override LocalizedString Category => T["Primitives"];
        public override LocalizedString Description => T["Evaluates an expression and stores the result into the workflow's output."];

        public string OutputName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<object> Expression
        {
            get => GetProperty(() => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var value = workflowContext.Evaluate(Expression);
            workflowContext.Output[OutputName] = value;

            yield return "Done";
        }
    }
}