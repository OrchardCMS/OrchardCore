using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class ForEachTask : TaskActivity
    {
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        protected readonly IStringLocalizer S;

        public ForEachTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ForEachTask> localizer)
        {
            _scriptEvaluator = scriptEvaluator;
            S = localizer;
        }
        public override string Name => nameof(ForEachTask);

        public override LocalizedString DisplayText => S["For Each Task"];

        public override LocalizedString Category => S["Control Flow"];

        /// <summary>
        /// An expression evaluating to an enumerable object to iterate over.
        /// </summary>
        public WorkflowExpression<IEnumerable<object>> Enumerable
        {
            get => GetProperty(() => new WorkflowExpression<IEnumerable<object>>());
            set => SetProperty(value);
        }

        /// <summary>
        /// The current iteration value.
        /// </summary>
        public string LoopVariableName
        {
            get => GetProperty(() => "x");
            set => SetProperty(value);
        }

        /// <summary>
        /// The current iteration value.
        /// </summary>
        public object Current
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }

        /// <summary>
        /// The current number of iterations executed.
        /// </summary>
        public int Index
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Iterate"], S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var items = (await _scriptEvaluator.EvaluateAsync(Enumerable, workflowContext)).ToList();
            var count = items.Count;

            if (Index < count)
            {
                var current = Current = items[Index];

                // TODO: Implement nested scopes. See https://github.com/OrchardCMS/OrchardCore/projects/4#card-6992776
                workflowContext.Properties[LoopVariableName] = current;
                workflowContext.LastResult = current;
                Index++;
                return Outcomes("Iterate");
            }
            else
            {
                Index = 0;
                return Outcomes("Done");
            }
        }
    }
}
