using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using OrchardCore.Users.TimeZone.Services;
using OrchardCore.Users.TimeZone.ViewModels;

namespace OrchardCore.Users.TimeZone.Drivers;

public class UserTimeZoneDisplayDriver : SectionDisplayDriver<User, UserTimeZone>
{
    private readonly UserTimeZoneService _userTimeZoneService;
    private readonly ISiteService _siteService;
    private readonly IClock _clock;

    public UserTimeZoneDisplayDriver(UserTimeZoneService userTimeZoneService, ISiteService siteService, IClock clock)
    {
        _userTimeZoneService = userTimeZoneService;
        _siteService = siteService;
        _clock = clock;
    }

    public override IDisplayResult Edit(UserTimeZone userTimeZone, BuildEditorContext context)
    {
        return Initialize<UserTimeZoneViewModel>("UserTimeZone_Edit", async model =>
        {
            var timeZoneId = userTimeZone.TimeZoneId;
            if (timeZoneId == null)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();

                timeZoneId = siteSettings.TimeZoneId;
            }

            model.TimeZoneId = timeZoneId;
            model.TimeZones = _clock.GetTimeZones().Select(tz =>
                new SelectListItem
                {
                    Text = tz.ToString(),
                    Value = tz.TimeZoneId,
                    Selected = tz.TimeZoneId == timeZoneId
                });
        }).Location("Content:3");
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserTimeZone userTimeZone, IUpdateModel updater, BuildEditorContext context)
    {
        var model = new UserTimeZoneViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            userTimeZone.TimeZoneId = model.TimeZoneId;

            // Remove the cache entry, don't update it, as the form might still fail validation for other reasons.
            await _userTimeZoneService.ClearUserTimeZoneCacheAsync(user);
        }

        return await EditAsync(userTimeZone, context);
    }
}
