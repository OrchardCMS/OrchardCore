using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetOutputTaskDisplay : ActivityDisplayDriver<SetOutputTask, SetOutputTaskViewModel>
    {
        protected override void Map(SetOutputTask source, SetOutputTaskViewModel target)
        {
            target.OutputName = source.OutputName;
            target.Expression = source.ScriptExpression.Expression;
        }

        protected override void Map(SetOutputTaskViewModel source, SetOutputTask target)
        {
            target.OutputName = source.OutputName.Trim();
            target.ScriptExpression = new WorkflowExpression<object>(source.Expression);
        }
    }
}
