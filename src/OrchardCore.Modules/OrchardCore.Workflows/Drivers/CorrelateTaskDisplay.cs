using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class CorrelateTaskDisplay : ActivityDisplayDriver<CorrelateTask, CorrelateTaskViewModel>
    {
        protected override void Map(CorrelateTask source, CorrelateTaskViewModel target)
        {
            target.Value = source.Value.Expression;
        }

        protected override void Map(CorrelateTaskViewModel source, CorrelateTask target)
        {
            target.Value = new WorkflowExpression<string>(source.Value);
        }
    }
}
