using System.Threading.Tasks;
using OrchardCore.UserProfile.Models;
using OrchardCore.UserProfile.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Users.Models;

namespace OrchardCore.UserProfile.Drivers
{
    public class UserProfileDisplayDriver : SectionDisplayDriver<User, UserProfile>
    {
        public override IDisplayResult Edit(UserProfile profile, BuildEditorContext context)
        {
            return Initialize<EditUserProfileViewModel>("UserProfile_Edit", model =>
            {
                model.TimeZone = profile.TimeZone;
            }).Location("Content:2");
        }

        public override async Task<IDisplayResult> UpdateAsync(UserProfile profile, IUpdateModel updater, BuildEditorContext context)
        {
            var model = new EditUserProfileViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                profile.TimeZone = model.TimeZone;
            }

            return Edit(profile);
        }
    }
}
