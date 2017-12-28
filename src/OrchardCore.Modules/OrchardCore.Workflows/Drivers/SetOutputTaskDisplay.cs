using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SetOutputTaskDisplay : ActivityDisplayDriver<SetOutputTask>
    {
        public override IDisplayResult Display(SetOutputTask activity)
        {
            return Combine(
                Shape("SetOutputTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("SetOutputTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(SetOutputTask activity)
        {
            return Shape<SetOutputTaskViewModel>("SetOutputTask_Fields_Edit", model =>
            {
                model.OutputName = activity.OutputName;
                model.Expression = activity.Expression.Expression;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(SetOutputTask activity, IUpdateModel updater)
        {
            var viewModel = new SetOutputTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                activity.OutputName = viewModel.OutputName.Trim();
                activity.Expression = new WorkflowExpression<object>(viewModel.Expression);
            }
            return Edit(activity);
        }
    }
}
