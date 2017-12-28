using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetVariableTaskDisplay : ActivityDisplayDriver<SetVariableTask>
    {
        public override IDisplayResult Display(SetVariableTask activity)
        {
            return Combine(
                Shape("SetVariableTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("SetVariableTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(SetVariableTask activity)
        {
            return Shape<SetVariableTaskViewModel>("SetVariableTask_Fields_Edit", model =>
            {
                model.VariableName = activity.VariableName;
                model.VariableValueExpression = activity.VariableValueExpression.Expression;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(SetVariableTask activity, IUpdateModel updater)
        {
            var viewModel = new SetVariableTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                activity.VariableName = viewModel.VariableName.Trim();
                activity.VariableValueExpression = new WorkflowExpression<object>(viewModel.VariableValueExpression);
            }
            return Edit(activity);
        }
    }
}
