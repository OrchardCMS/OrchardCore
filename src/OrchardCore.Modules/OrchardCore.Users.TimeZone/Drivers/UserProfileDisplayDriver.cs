using System.Threading.Tasks;
using OrchardCore.Users.TimeZone.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Users.Models;
using OrchardCore.Modules;
using OrchardCore.Users.Services;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users.TimeZone.Drivers
{
    public class UserProfileDisplayDriver : SectionDisplayDriver<User, Models.UserProfile>
    {
        public const string GroupId = "UserProfile";
        private readonly IClock _clock;
        private readonly IUserTimeZoneService _userTimeZoneService;

        public UserProfileDisplayDriver(
            IClock clock,
            IUserTimeZoneService userTimeZoneService) {
            _clock = clock;
            _userTimeZoneService = userTimeZoneService;
        }

        public override IDisplayResult Edit(Models.UserProfile profile, BuildEditorContext context)
        {
            return Initialize<EditUserProfileViewModel>("UserProfile_Edit", model =>
            {
                model.TimeZone = profile.TimeZone;
                model.TimeZones = _clock.GetTimeZones();
            }).Location("Content:2");
        }

        public override async Task<IDisplayResult> UpdateAsync(Models.UserProfile profile, IUpdateModel updater, BuildEditorContext context)
        {
            var model = new EditUserProfileViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                profile.TimeZone = model.TimeZone;
            }

            await _userTimeZoneService.SetSiteTimeZoneAsync(profile.TimeZone);
            
            return Edit(profile);
        }
    }
}
