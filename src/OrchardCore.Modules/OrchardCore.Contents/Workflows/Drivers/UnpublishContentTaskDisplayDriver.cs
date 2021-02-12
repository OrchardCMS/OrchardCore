using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class UnpublishContentTaskDisplayDriver : ContentTaskDisplayDriver<UnpublishContentTask, UnpublishContentTaskViewModel>
    {
        protected override void EditActivity(UnpublishContentTask activity, UnpublishContentTaskViewModel model)
        {
            model.Expression = activity.Content.Expression;
        }

        protected override void UpdateActivity(UnpublishContentTaskViewModel model, UnpublishContentTask activity)
        {
            activity.Content = new WorkflowExpression<IContent>(model.Expression);
        }
    }
}
