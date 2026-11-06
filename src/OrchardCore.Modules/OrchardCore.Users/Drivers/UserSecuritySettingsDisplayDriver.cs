using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class UserSecuritySettingsDisplayDriver : SiteDisplayDriver<UserSecuritySettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public UserSecuritySettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override IDisplayResult Edit(ISite site, UserSecuritySettings settings, BuildEditorContext context)
    {
        return Initialize<UserSecuritySettings>("UserSecuritySettings_Edit", model =>
        {
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location("Content:10#Security Page")
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
