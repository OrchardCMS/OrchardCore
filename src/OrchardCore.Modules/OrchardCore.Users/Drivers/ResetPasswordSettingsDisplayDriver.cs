using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

    internal readonly IStringLocalizer S;

    public ResetPasswordSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ResetPasswordSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ResetPasswordSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        return Initialize<ResetPasswordSettings>("ResetPasswordSettings_Edit", model =>
        {
            model.AllowResetPassword = settings.AllowResetPassword;
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location(l => l.Zone("Content", "5").Card(S["Password reset"], "10"))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ResetPasswordSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
