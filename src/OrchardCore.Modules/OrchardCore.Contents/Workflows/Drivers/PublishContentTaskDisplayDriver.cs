using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class PublishContentTaskDisplayDriver : ContentTaskDisplayDriver<PublishContentTask, PublishContentTaskViewModel>
    {
        protected override void EditActivity(PublishContentTask activity, PublishContentTaskViewModel model)
        {
            model.Expression = activity.Content.Expression;
        }

        protected override void UpdateActivity(PublishContentTaskViewModel model, PublishContentTask activity)
        {
            activity.Content = new WorkflowExpression<IContent>(model.Expression);
        }
    }
}
