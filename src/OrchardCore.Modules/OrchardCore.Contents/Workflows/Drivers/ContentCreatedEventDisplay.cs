using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentCreatedEventDisplay : ActivityDisplayDriver<ContentCreatedEvent>
    {
        public override IDisplayResult Display(ContentCreatedEvent model)
        {
            return Shape("ContentCreatedEvent_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
