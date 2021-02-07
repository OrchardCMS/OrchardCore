using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetOutputTaskDisplayDriver : ActivityDisplayDriver<SetOutputTask, SetOutputTaskViewModel>
    {
        protected override void EditActivity(SetOutputTask source, SetOutputTaskViewModel model)
        {
            model.OutputName = source.OutputName;
            model.Value = source.Value.Expression;
        }

        protected override void UpdateActivity(SetOutputTaskViewModel model, SetOutputTask activity)
        {
            activity.OutputName = model.OutputName.Trim();
            activity.Value = new WorkflowExpression<object>(model.Value);
        }
    }
}
