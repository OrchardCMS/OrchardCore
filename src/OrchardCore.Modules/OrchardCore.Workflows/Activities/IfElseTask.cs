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
        protected readonly IStringLocalizer S;

        public IfElseTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<IfElseTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            S = localizer;
        }
        public override string Name => nameof(IfElseTask);

        public override LocalizedString DisplayText => S["If Else Task"];

        public override LocalizedString Category => S["Control Flow"];

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
            return Outcomes(S["True"], S["False"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var result = await _scriptEvaluator.EvaluateAsync(Condition, workflowContext);
            return Outcomes(result ? "True" : "False");
        }
    }
}
