using OrchardCore.Workflows.Display;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Contents.Workflows.Activities;

namespace OrchardCore.Contents.Workflows.Drivers
{


    public class ContentForEachTaskDisplayDriver: ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        protected override void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.ContentType = activity.ContentType;
        }

        protected override void UpdateActivity(ContentForEachTaskViewModel model, ContentForEachTask activity)
        {
            activity.ContentType = model.ContentType;
        }
    }

}
