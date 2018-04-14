using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class IfElseTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;

        public IfElseTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<IfElseTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(IfElseTask);
        public override LocalizedString Category => T["Control Flow"];

        /// <summary>
        /// A script evaluating to either true or false.
        /// </summary>
        public WorkflowExpression<bool> Condition
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["True"], T["False"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var result = await _scriptEvaluator.EvaluateAsync(Condition, workflowContext);
            return Outcomes(result ? "True" : "False");
        }
    }
}