using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class RetrieveContentTaskDisplayDriver : ContentTaskDisplayDriver<RetrieveContentTask, RetrieveContentTaskViewModel>
    {
        protected override void EditActivity(RetrieveContentTask activity, RetrieveContentTaskViewModel model)
        {
            model.ContentItemId = activity.Content.Expression;
        }

        protected override void UpdateActivity(RetrieveContentTaskViewModel model, RetrieveContentTask activity)
        {
            activity.Content = new WorkflowExpression<IContent>(model.ContentItemId);
        }
    }
}
