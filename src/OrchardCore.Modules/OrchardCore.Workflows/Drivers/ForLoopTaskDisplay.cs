using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForLoopTaskDisplay : ActivityDisplayDriver<ForLoopTask>
    {
        public override IDisplayResult Display(ForLoopTask activity)
        {
            return Combine(
                Shape("ForLoopTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("ForLoopTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(ForLoopTask activity)
        {
            return Shape<ForLoopTaskViewModel>("ForLoopTask_Fields_Edit", model =>
            {
                model.CountExpression = activity.CountExpression.Expression;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(ForLoopTask activity, IUpdateModel updater)
        {
            var viewModel = new ForLoopTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                activity.CountExpression = new WorkflowExpression<int>(viewModel.CountExpression);
            }
            return Edit(activity);
        }
    }
}
