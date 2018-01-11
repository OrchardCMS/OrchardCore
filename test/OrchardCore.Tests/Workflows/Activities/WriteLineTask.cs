using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Workflows.Activities
{
    public class WriteLineTask : TaskActivity
    {
        private readonly TextWriter _output;

        public WriteLineTask(IStringLocalizer t, TextWriter output)
        {
            _output = output;
            T = t;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(WriteLineTask);
        public override LocalizedString Category => T["Test"];
        public override LocalizedString Description => T["Writes out the specified text."];

        public WorkflowExpression<string> Text
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<IEnumerable<string>> ExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var text = await workflowContext.EvaluateExpressionAsync(Text);
            _output.WriteLine(text);
            return new[] { "Done" };
        }
    }
}
