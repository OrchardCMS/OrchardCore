using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalUserRoleLoginSettingsDisplayDriver : SiteDisplayDriver<ExternalUserRoleLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ExternalUserRoleLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => ExternalUserLoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, ExternalUserRoleLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<ExternalUserRoleLoginSettings>("ExternalUserRoleLoginSettings_Edit", model =>
        {
            model.UseScriptToSyncRoles = settings.UseScriptToSyncRoles;
            model.SyncRolesScript = settings.SyncRolesScript;
        }).Location("Content:10#General")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ManageUsers))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalUserRoleLoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        return await EditAsync(site, section, context);
    }
}
