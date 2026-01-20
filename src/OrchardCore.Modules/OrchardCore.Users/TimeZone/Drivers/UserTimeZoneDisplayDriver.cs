using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using OrchardCore.Users.TimeZone.Services;
using OrchardCore.Users.TimeZone.ViewModels;

namespace OrchardCore.Users.TimeZone.Drivers
{
    public class UserTimeZoneDisplayDriver : SectionDisplayDriver<User, UserTimeZone>
    {
        private readonly UserTimeZoneService _userTimeZoneService;

        public UserTimeZoneDisplayDriver(UserTimeZoneService userTimeZoneService)
        {
            _userTimeZoneService = userTimeZoneService;
        }

        public override IDisplayResult Edit(UserTimeZone userTimeZone, BuildEditorContext context)
        {
            return Initialize<UserTimeZoneViewModel>("UserTimeZone_Edit", model =>
            {
                model.TimeZoneId = userTimeZone.TimeZoneId;
            }).Location("Content:2");
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UserTimeZone userTimeZone, IUpdateModel updater, BuildEditorContext context)
        {
            var model = new UserTimeZoneViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                userTimeZone.TimeZoneId = model.TimeZoneId;

                // Remove the cache entry, don't update it, as the form might still fail validation for other reasons.
                await _userTimeZoneService.UpdateUserTimeZoneAsync(user);
            }

            return await EditAsync(userTimeZone, context);
        }
    }
}
