using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForLoopTaskDisplay : ActivityDisplayDriver<ForLoopTask, ForLoopTaskViewModel>
    {
        protected override void Map(ForLoopTask source, ForLoopTaskViewModel target)
        {
            target.CountExpression = source.CountExpression.Expression;
        }

        protected override void Map(ForLoopTaskViewModel source, ForLoopTask target)
        {
            target.CountExpression = new WorkflowExpression<int>(source.CountExpression);
        }
    }
}
