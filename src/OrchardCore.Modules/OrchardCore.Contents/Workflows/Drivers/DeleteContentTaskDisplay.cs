using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class DeleteContentTaskDisplay : ActivityDisplayDriver<DeleteContentTask>
    {
        public override IDisplayResult Display(DeleteContentTask model)
        {
            return Shape("DeleteContentTask_Fields_Thumbnail").Location("Thumbnail", "Content");
        }
    }
}
