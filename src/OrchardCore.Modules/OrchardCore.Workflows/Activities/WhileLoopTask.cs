using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class WhileLoopTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        protected readonly IStringLocalizer S;

        public WhileLoopTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<WhileLoopTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            S = localizer;
        }

        public override string Name => nameof(WhileLoopTask);

        public override LocalizedString DisplayText => S["While Loop Task"];

        public override LocalizedString Category => S["Control Flow"];

        /// <summary>
        /// An expression evaluating to true or false.
        /// </summary>
        public WorkflowExpression<bool> Condition
        {
            get => GetProperty(() => new WorkflowExpression<bool>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Iterate"], S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var loop = await _scriptEvaluator.EvaluateAsync(Condition, workflowContext);
            return Outcomes(loop ? "Iterate" : "Done");
        }
    }
}
