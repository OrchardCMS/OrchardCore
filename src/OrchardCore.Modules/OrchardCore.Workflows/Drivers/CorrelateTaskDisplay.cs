using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class CorrelateTaskDisplay : ActivityDisplayDriver<CorrelateTask, CorrelateTaskViewModel>
    {
        protected override void Map(CorrelateTask source, CorrelateTaskViewModel target)
        {
            target.Expression = source.Expression.Expression;
        }

        protected override void Map(CorrelateTaskViewModel source, CorrelateTask target)
        {
            target.Expression = new WorkflowExpression<string>(source.Expression);
        }
    }
}
