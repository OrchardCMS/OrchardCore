using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class DeleteContentTaskDisplayDriver : ContentTaskDisplayDriver<DeleteContentTask, DeleteContentTaskViewModel>
    {
        protected override void EditActivity(DeleteContentTask activity, DeleteContentTaskViewModel model)
        {
            model.Expression = activity.Content.Expression;
        }

        protected override void UpdateActivity(DeleteContentTaskViewModel model, DeleteContentTask activity)
        {
            activity.Content = new WorkflowExpression<IContent>(model.Expression);
        }
    }
}
