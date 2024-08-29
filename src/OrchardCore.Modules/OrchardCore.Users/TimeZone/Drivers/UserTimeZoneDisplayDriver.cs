using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using OrchardCore.Users.TimeZone.Services;
using OrchardCore.Users.TimeZone.ViewModels;

namespace OrchardCore.Users.TimeZone.Drivers;

public sealed class UserTimeZoneDisplayDriver : SectionDisplayDriver<User, UserTimeZone>
{
    private readonly IUserTimeZoneService _userTimeZoneService;

    public UserTimeZoneDisplayDriver(IUserTimeZoneService userTimeZoneService)
    {
        _userTimeZoneService = userTimeZoneService;
    }

    public override IDisplayResult Edit(User user, UserTimeZone userTimeZone, BuildEditorContext context)
    {
        return Initialize<UserTimeZoneViewModel>("UserTimeZone_Edit", model =>
        {
            model.TimeZoneId = userTimeZone.TimeZoneId;
        }).Location("Content:2");
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserTimeZone userTimeZone, UpdateEditorContext context)
    {
        var model = new UserTimeZoneViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);
        userTimeZone.TimeZoneId = model.TimeZoneId;

        // Remove the cache entry, don't update it, as the form might still fail validation for other reasons.
        await _userTimeZoneService.UpdateAsync(user);

        return await EditAsync(user, userTimeZone, context);
    }
}
