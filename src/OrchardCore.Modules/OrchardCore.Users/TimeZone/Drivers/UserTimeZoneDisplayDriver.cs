using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Models;
using OrchardCore.Users.TimeZone.ViewModels;

namespace OrchardCore.Users.TimeZone.Drivers;

public sealed class UserTimeZoneDisplayDriver : SectionDisplayDriver<User, UserTimeZone>
{
    internal readonly IStringLocalizer S;

    public UserTimeZoneDisplayDriver(IStringLocalizer<UserTimeZoneDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(User user, UserTimeZone userTimeZone, BuildEditorContext context)
    {
        return Initialize<UserTimeZoneViewModel>("UserTimeZone_Edit", model =>
        {
            model.TimeZoneId = userTimeZone.TimeZoneId;
        }).Location(l => l.Zone("Content", "2").Card(S["Regional settings"], "40"));
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserTimeZone userTimeZone, UpdateEditorContext context)
    {
        var model = new UserTimeZoneViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        userTimeZone.TimeZoneId = model.TimeZoneId;

        return await EditAsync(user, userTimeZone, context);
    }
}
