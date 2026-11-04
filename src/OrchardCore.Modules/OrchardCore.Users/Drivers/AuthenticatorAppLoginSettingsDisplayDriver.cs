using OrchardCore.Users.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class AuthenticatorAppLoginSettingsDisplayDriver : SiteDisplayDriver<AuthenticatorAppLoginSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public AuthenticatorAppLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, AuthenticatorAppLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<AuthenticatorAppLoginSettings>("AuthenticatorAppLoginSettings_Edit", model =>
        {
            model.UseEmailAsAuthenticatorDisplayName = settings.UseEmailAsAuthenticatorDisplayName;
            model.TokenLength = settings.TokenLength;
        }).Location("Content:12#Two-Factor Authentication")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AuthenticatorAppLoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        var model = UserPolicySettingsEditor.Clone(section);
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        model.TokenLength = 6;
        if (context.Updater.ModelState.IsValid)
        {
            UserPolicySettingsEditor.Apply(section, model);
        }

        return Edit(site, section, context);
    }
}
