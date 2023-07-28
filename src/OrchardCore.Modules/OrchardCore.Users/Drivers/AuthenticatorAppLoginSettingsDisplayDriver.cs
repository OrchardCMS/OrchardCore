using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public class AuthenticatorAppLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, AuthenticatorAppLoginSettings>
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
    public override IDisplayResult Edit(AuthenticatorAppLoginSettings settings)
    {
        return Initialize<AuthenticatorAppLoginSettings>("AuthenticatorAppLoginSettings_Edit", model =>
        {
            model.UseEmailAsAuthenticatorDisplayName = settings.UseEmailAsAuthenticatorDisplayName;
            model.TokenLength = settings.TokenLength;
        }).Location("Content:12#Two-Factor Authentication")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        .OnGroup(LoginSettingsDisplayDriver.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(AuthenticatorAppLoginSettings section, BuildEditorContext context)
    {
        if (!context.GroupId.Equals(LoginSettingsDisplayDriver.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
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

        return Edit(section);
    }
}
