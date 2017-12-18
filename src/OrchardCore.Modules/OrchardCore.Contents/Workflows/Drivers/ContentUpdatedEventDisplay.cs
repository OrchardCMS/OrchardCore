using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentUpdatedEventDisplay : ActivityDisplayDriver<ContentUpdatedEvent>
    {
        public override IDisplayResult Display(ContentUpdatedEvent model)
        {
            return Shape("ContentUpdatedEvent_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
