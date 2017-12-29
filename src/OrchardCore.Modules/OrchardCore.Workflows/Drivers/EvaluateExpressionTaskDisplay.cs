using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class EvaluateExpressionTaskDisplay : ActivityDisplayDriver<EvaluateExpressionTask, EvaluateExpressionTaskViewModel>
    {
        protected override void Map(EvaluateExpressionTask source, EvaluateExpressionTaskViewModel target)
        {
            target.Expression = source.Expression.Expression;
        }

        protected override void Map(EvaluateExpressionTaskViewModel source, EvaluateExpressionTask target)
        {
            target.Expression = new WorkflowExpression<object>(source.Expression);
        }
    }
}
