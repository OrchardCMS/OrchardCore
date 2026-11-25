using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

    internal readonly IStringLocalizer S;

    public AuthenticatorAppLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<AuthenticatorAppLoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => LoginSettingsDisplayDriver.GroupId;

    public override IDisplayResult Edit(ISite site, AuthenticatorAppLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<AuthenticatorAppLoginSettings>("AuthenticatorAppLoginSettings_Edit", model =>
        {
            model.UseEmailAsAuthenticatorDisplayName = settings.UseEmailAsAuthenticatorDisplayName;
            model.TokenLength = settings.TokenLength;
        }).Location(l => l.Zone("Content", "12").Tab("Two-Factor Authentication").Card(S["Authenticator app"], "40"))
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers), this)
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AuthenticatorAppLoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
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
