using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Activities
{
    public class SetPropertyTask : TaskActivity
    {
        public SetPropertyTask(IStringLocalizer<SetPropertyTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(SetPropertyTask);
        public override LocalizedString Category => T["Primitives"];
        public override LocalizedString Description => T["Assigns a value to a property on the workflow."];

        public string PropertyName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<object> ScriptExpression
        {
            get => GetProperty(() => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var value = await workflowContext.EvaluateScriptAsync(ScriptExpression);
            workflowContext.Properties[PropertyName] = value;

            return Outcomes("Done");
        }
    }
}