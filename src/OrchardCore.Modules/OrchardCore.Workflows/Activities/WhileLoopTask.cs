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

        public WhileLoopTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<WhileLoopTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            T = localizer;
        }
        
        private IStringLocalizer T { get; }

        public override string Name => nameof(WhileLoopTask);
        public override LocalizedString Category => T["Control Flow"];

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
            return Outcomes(T["Iterate"], T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var loop = await _scriptEvaluator.EvaluateAsync(Condition, workflowContext);
            return Outcomes(loop ? "Iterate" : "Done");
        }
    }
}