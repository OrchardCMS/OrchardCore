using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentDeletedEventDisplay : ActivityDisplayDriver<ContentDeletedEvent>
    {
        public override IDisplayResult Display(ContentDeletedEvent model)
        {
            return Shape("ContentDeletedEvent_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
