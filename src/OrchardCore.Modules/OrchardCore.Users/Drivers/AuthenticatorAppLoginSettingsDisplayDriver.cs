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
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AuthenticatorAppLoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        // A possible issue in Identity prevents from validation token that are not 6 in length.
        // If this limitation is lifted, the following block can be uncommented.
        // For more info read https://github.com/dotnet/aspnetcore/issues/48317
        /*
        if (section.TokenLength != 6 && section.TokenLength != 8)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(section.TokenLength), S["The token length should be either 6 or 8."]);
        }
        */

        return Edit(site, section, context);
    }
}
