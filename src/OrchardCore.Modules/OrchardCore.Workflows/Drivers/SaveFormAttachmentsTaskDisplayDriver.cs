using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SaveFormAttachmentsTaskDisplayDriver : ActivityDisplayDriver<SaveFormAttachmentsTask, SaveFormAttachmentsTaskViewModel>
    {
        protected override void EditActivity(SaveFormAttachmentsTask activity, SaveFormAttachmentsTaskViewModel model)
        {
            model.Folder = activity.Folder;
            model.UseMediaFileStore = activity.UseMediaFileStore;
        }

        protected override void UpdateActivity(SaveFormAttachmentsTaskViewModel model, SaveFormAttachmentsTask activity)
        {
            activity.Folder = model.Folder;
            activity.UseMediaFileStore = model.UseMediaFileStore;
        }
    }
}
