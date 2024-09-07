using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class RegistrationSettingsDisplayDriver : SiteDisplayDriver<RegistrationSettings>
{
    public const string GroupId = "userRegistration";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public RegistrationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
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

        return Initialize<RegistrationSettings>("RegistrationSettings_Edit", model =>
        {
            model.UsersCanRegister = settings.UsersCanRegister;
            model.UsersMustValidateEmail = settings.UsersMustValidateEmail;
            model.UsersAreModerated = settings.UsersAreModerated;
            model.UseSiteTheme = settings.UseSiteTheme;
            model.NoPasswordForExternalUsers = settings.NoPasswordForExternalUsers;
            model.NoUsernameForExternalUsers = settings.NoUsernameForExternalUsers;
            model.NoEmailForExternalUsers = settings.NoEmailForExternalUsers;
            model.UseScriptToGenerateUsername = settings.UseScriptToGenerateUsername;
            model.GenerateUsernameScript = settings.GenerateUsernameScript;
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

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
