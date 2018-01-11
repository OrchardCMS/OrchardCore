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
            target.ScriptExpression = source.ScriptExpression.Expression;
        }

        protected override void Map(SetPropertyTaskViewModel source, SetPropertyTask target)
        {
            target.PropertyName = source.PropertyName.Trim();
            target.ScriptExpression = new WorkflowExpression<object>(source.ScriptExpression);
        }
    }
}
