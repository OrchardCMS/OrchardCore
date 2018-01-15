using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class SetOutputTask : TaskActivity
    {
        public SetOutputTask(IStringLocalizer<SetOutputTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(SetOutputTask);
        public override LocalizedString Category => T["Primitives"];
        public override LocalizedString Description => T["Evaluates a script expression and stores the result into the workflow's output."];

        public string OutputName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<object> ScriptExpression
        {
            get => GetProperty(() => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var value = await workflowContext.EvaluateScriptAsync(ScriptExpression);
            workflowContext.Output[OutputName] = value;

            return Outcomes("Done");
        }
    }
}