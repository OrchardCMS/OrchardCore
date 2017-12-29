using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetOutputTaskDisplay : ActivityDisplayDriver<SetOutputTask, SetOutputTaskViewModel>
    {
        protected override void Map(SetOutputTask source, SetOutputTaskViewModel target)
        {
            target.OutputName = source.OutputName;
            target.Expression = source.Expression.Expression;
        }

        protected override void Map(SetOutputTaskViewModel source, SetOutputTask target)
        {
            target.OutputName = source.OutputName.Trim();
            target.Expression = new WorkflowExpression<object>(source.Expression);
        }
    }
}
