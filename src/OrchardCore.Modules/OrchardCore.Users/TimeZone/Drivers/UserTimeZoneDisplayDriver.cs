using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using OrchardCore.Users.TimeZone.Services;
using OrchardCore.Users.TimeZone.ViewModels;

namespace OrchardCore.Users.TimeZone.Drivers
{
    public class UserTimeZoneDisplayDriver : SectionDisplayDriver<User, UserTimeZone>
    {
        private readonly IClock _clock;
        private readonly UserTimeZoneService _userTimeZoneService;

        public UserTimeZoneDisplayDriver(
            IClock clock,
            UserTimeZoneService userTimeZoneService)
        {
            _clock = clock;
            _userTimeZoneService = userTimeZoneService;
        }

        public override Task<IDisplayResult> EditAsync(UserTimeZone userTimeZone, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<UserTimeZoneViewModel>("UserProfile_Edit", model =>
                {
                    model.TimeZone = userTimeZone.TimeZoneId;
                }).Location("Content:2")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(UserTimeZone userTimeZone, IUpdateModel updater, BuildEditorContext context)
        {
            var model = new UserTimeZoneViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                userTimeZone.TimeZoneId = model.TimeZone;
            }

            await _userTimeZoneService.UpdateUserTimeZoneAsync(userTimeZone);

            return await EditAsync(userTimeZone, context);
        }
    }
}
