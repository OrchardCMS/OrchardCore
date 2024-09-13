using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalAuthenticationSettingsDisplayDriver : SiteDisplayDriver<ExternalAuthenticationSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ExternalAuthenticationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => RegistrationSettingsDisplayDriver.GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ExternalAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
        {
            return null;
        }

        return Initialize<ExternalAuthenticationSettings>("ExternalAuthenticationSettings_Edit", model =>
        {
            model.NoPassword = settings.NoPassword;
            model.NoUsername = settings.NoUsername;
            model.NoEmail = settings.NoEmail;
            model.UseScriptToGenerateUsername = settings.UseScriptToGenerateUsername;
            model.GenerateUsernameScript = settings.GenerateUsernameScript;
        }).Location("Content:10")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalAuthenticationSettings settings, UpdateEditorContext context)
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
