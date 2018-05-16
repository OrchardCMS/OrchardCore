using System.Threading.Tasks;
using OrchardCore.Users.TimeZone.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Users.Models;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Drivers
{
    public class UserProfileDisplayDriver : SectionDisplayDriver<User, Models.UserProfile>
    {
        private readonly IClock _clock;

        public UserProfileDisplayDriver(IClock clock) {
            _clock = clock;
        }

        public override IDisplayResult Edit(Models.UserProfile profile, BuildEditorContext context)
        {
            return Initialize<EditUserProfileViewModel>("UserProfile_Edit", model =>
            {
                model.TimeZone = profile.TimeZone;
                model.TimeZones = _clock.GetTimeZones(string.Empty);
            }).Location("Content:2");
        }

        public override async Task<IDisplayResult> UpdateAsync(Models.UserProfile profile, IUpdateModel updater, BuildEditorContext context)
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
