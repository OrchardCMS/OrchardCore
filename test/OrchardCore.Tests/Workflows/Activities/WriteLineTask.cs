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
        protected readonly IStringLocalizer S;

        public WriteLineTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer stringLocalizer, TextWriter output)
        {
            _scriptEvaluator = scriptEvaluator;
            _output = output;
            S = stringLocalizer;
        }

        public override string Name => nameof(WriteLineTask);

        public override LocalizedString DisplayText => S["Write Line Task"];

        public override LocalizedString Category => S["Test"];

        public WorkflowExpression<string> Text
        {
            get => GetProperty<WorkflowExpression<string>>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var text = await _scriptEvaluator.EvaluateAsync(Text, workflowContext);
            await _output.WriteLineAsync(text);
            return Outcomes("Done");
        }
    }
}
