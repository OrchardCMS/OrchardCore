using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class DeleteContentTaskDisplay : ActivityDisplayDriver<DeleteContentTask>
    {
        public override IDisplayResult Display(DeleteContentTask activity)
        {
            return Combine(
                Shape("DeleteContentTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("DeleteContentTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(DeleteContentTask activity)
        {
            return Shape<DeleteContentTaskViewModel>("DeleteContentTask_Fields_Edit", model =>
            {
                model.Expression = activity.Content.Expression;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(DeleteContentTask activity, IUpdateModel updater)
        {
            var viewModel = new DeleteContentTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, x => x.Expression))
            {
                activity.Content = new WorkflowExpression<IContent>(viewModel.Expression);
            }
            return Edit(activity);
        }
    }
}
