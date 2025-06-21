using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ResetPasswordSettingsDisplayDriver : SiteDisplayDriver<ResetPasswordSettings>
{
    public const string GroupId = "userResetPassword";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ResetPasswordSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ResetPasswordSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers).ConfigureAwait(false))
        {
            return null;
        }

        return Initialize<ResetPasswordSettings>("ResetPasswordSettings_Edit", model =>
        {
            model.AllowResetPassword = settings.AllowResetPassword;
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ResetPasswordSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers).ConfigureAwait(false))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix).ConfigureAwait(false);

        return await EditAsync(site, settings, context).ConfigureAwait(false);
    }
}
