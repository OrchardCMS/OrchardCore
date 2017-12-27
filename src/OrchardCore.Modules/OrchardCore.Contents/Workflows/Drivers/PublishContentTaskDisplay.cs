using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class PublishContentTaskDisplay : ActivityDisplayDriver<PublishContentTask>
    {
        public override IDisplayResult Display(PublishContentTask model)
        {
            return Shape("PublishContentTask_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
