using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetVariableTaskDisplay : ActivityDisplayDriver<SetVariableTask, SetVariableTaskViewModel>
    {
        protected override void Map(SetVariableTask source, SetVariableTaskViewModel target)
        {
            target.VariableName = source.VariableName;
            target.Expression = source.Expression.Expression;
        }

        protected override void Map(SetVariableTaskViewModel source, SetVariableTask target)
        {
            target.VariableName = source.VariableName.Trim();
            target.Expression = new WorkflowExpression<object>(source.Expression);
        }
    }
}
