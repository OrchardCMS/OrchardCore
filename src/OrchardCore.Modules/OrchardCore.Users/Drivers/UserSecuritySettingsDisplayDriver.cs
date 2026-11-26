using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

internal sealed class UserSecuritySettingsDisplayDriver : SiteDisplayDriver<UserSecuritySettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public UserSecuritySettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<UserSecuritySettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, UserSecuritySettings settings, BuildEditorContext context)
    {
        return Initialize<UserSecuritySettings>("UserSecuritySettings_Edit", model =>
        {
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location(l => l.Zone("Content", "10").Tab("Security Page").Card(S["Theme"], "10"))
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, UserSecuritySettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return Edit(site, settings, context);
    }
}
