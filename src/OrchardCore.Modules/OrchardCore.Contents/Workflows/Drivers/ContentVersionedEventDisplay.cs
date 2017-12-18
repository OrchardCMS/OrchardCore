using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentVersionedEventDisplay : ActivityDisplayDriver<ContentVersionedEvent>
    {
        public override IDisplayResult Display(ContentVersionedEvent model)
        {
            return Shape("ContentVersionedEvent_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
