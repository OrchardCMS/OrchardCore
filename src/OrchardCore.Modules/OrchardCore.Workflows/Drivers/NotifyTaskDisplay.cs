using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class NotifyTaskDisplay : ActivityDisplayDriver<NotifyTask>
    {
        public override IDisplayResult Display(NotifyTask activity)
        {
            return Combine(
                Shape("NotifyTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("NotifyTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(NotifyTask activity)
        {
            return Shape<NotifyTaskViewModel>("NotifyTask_Fields_Edit", model =>
            {
                model.NotificationType = activity.NotificationType;
                model.Message = activity.Message;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(NotifyTask model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, x => x.NotificationType, x => x.Message);
            return Edit(model);
        }
    }
}
