using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class RegistrationSettingsDisplayDriver : SiteDisplayDriver<RegistrationSettings>
{
    public const string GroupId = "userRegistration";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;

    public RegistrationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, RegistrationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<RegistrationSettings>("RegistrationSettings_Edit", model =>
        {
            model.UsersMustValidateEmail = settings.UsersMustValidateEmail;
            model.UsersAreModerated = settings.UsersAreModerated;
            model.UseSiteTheme = settings.UseSiteTheme;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, RegistrationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
        {
            return null;
        }

        var model = new RegistrationSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var hasChange = model.UsersMustValidateEmail != settings.UsersMustValidateEmail
            || model.UsersAreModerated != settings.UsersAreModerated
            || model.UseSiteTheme != settings.UseSiteTheme;

        settings.UsersMustValidateEmail = model.UsersMustValidateEmail;
        settings.UsersAreModerated = model.UsersAreModerated;
        settings.UseSiteTheme = model.UseSiteTheme;

        if (hasChange)
        {
            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}
