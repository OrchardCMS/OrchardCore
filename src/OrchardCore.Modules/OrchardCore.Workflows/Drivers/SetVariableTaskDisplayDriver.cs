using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetVariableTaskDisplayDriver : ActivityDisplayDriver<SetPropertyTask, SetPropertyTaskViewModel>
    {
        protected override void EditActivity(SetPropertyTask source, SetPropertyTaskViewModel model)
        {
            model.PropertyName = source.PropertyName;
            model.Value = source.Value.Expression;
        }

        protected override void UpdateActivity(SetPropertyTaskViewModel model, SetPropertyTask activity)
        {
            activity.PropertyName = model.PropertyName.Trim();
            activity.Value = new WorkflowExpression<object>(model.Value);
        }
    }
}
