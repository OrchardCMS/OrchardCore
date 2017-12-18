using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentPublishedEventDisplay : ActivityDisplayDriver<ContentPublishedEvent>
    {
        public override IDisplayResult Display(ContentPublishedEvent model)
        {
            return Shape("ContentPublishedEvent_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
