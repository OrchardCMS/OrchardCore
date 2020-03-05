using System;
using System.Threading.Tasks;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Demo.Drivers
{
    public class UserProfileDisplayDriver : SectionDisplayDriver<User, UserProfile>
    {
        public override IDisplayResult Edit(UserProfile profile, BuildEditorContext context)
        {
            return Initialize<EditUserProfileViewModel>("UserProfile_Edit", model =>
            {
                model.Age = profile.Age;
                model.FirstName = profile.FirstName;
                model.LastName = profile.LastName;
            }).Location("Content:2");
        }

        public override async Task<IDisplayResult> UpdateAsync(UserProfile profile, BuildEditorContext context)
        {
            var model = new EditUserProfileViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                profile.Age = model.Age;
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.UpdatedAt = DateTime.UtcNow;
            }

            return Edit(profile, context);
        }
    }
}
