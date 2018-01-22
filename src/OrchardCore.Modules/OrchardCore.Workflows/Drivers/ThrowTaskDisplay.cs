using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ThrowTaskDisplay : ActivityDisplayDriver<ThrowTask, ThrowTaskViewModel>
    {
        protected override void Map(ThrowTask source, ThrowTaskViewModel target)
        {
            target.ExceptionType = source.ExceptionType;
            target.Message = source.Message.Expression;
        }

        protected override void Map(ThrowTaskViewModel source, ThrowTask target)
        {
            target.ExceptionType = source.ExceptionType;
            target.Message = new WorkflowExpression<string>(source.Message?.Trim());
        }
    }
}
