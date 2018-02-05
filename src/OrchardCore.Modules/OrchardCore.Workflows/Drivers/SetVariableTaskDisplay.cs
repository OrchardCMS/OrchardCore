using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetVariableTaskDisplay : ActivityDisplayDriver<SetPropertyTask, SetPropertyTaskViewModel>
    {
        protected override void Map(SetPropertyTask source, SetPropertyTaskViewModel target)
        {
            target.PropertyName = source.PropertyName;
            target.Value = source.Value.Expression;
        }

        protected override void Map(SetPropertyTaskViewModel source, SetPropertyTask target)
        {
            target.PropertyName = source.PropertyName.Trim();
            target.Value = new WorkflowExpression<object>(source.Value);
        }
    }
}
