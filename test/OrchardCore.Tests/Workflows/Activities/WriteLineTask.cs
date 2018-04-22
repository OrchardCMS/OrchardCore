using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Workflows.Activities
{
    public class WriteLineTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly TextWriter _output;

        public WriteLineTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer t, TextWriter output)
        {
            _scriptEvaluator = scriptEvaluator;
            _output = output;
            T = t;
        }

        private IStringLocalizer T { get; }
        public override string Name => nameof(WriteLineTask);
        public override LocalizedString Category => T["Test"];

        public WorkflowExpression<string> Text
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var text = await _scriptEvaluator.EvaluateAsync(Text, workflowContext);
            _output.WriteLine(text);
            return Outcomes("Done");
        }
    }
}
