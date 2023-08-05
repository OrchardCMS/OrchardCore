using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public class FacebookPixelSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookPixelSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FacebookPixelSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> EditAsync(FacebookPixelSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        return Initialize<FacebookPixelSettings>("FacebookPixelSettings_Edit", model =>
        {
            model.PixelId = settings.PixelId;
        }).Location("Content:0")
        .OnGroup(FacebookConstants.PixelSettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(FacebookPixelSettings settings, BuildEditorContext context)
    {
        if (!String.Equals(FacebookConstants.PixelSettingsGroupId, context.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageFacebookApp))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        settings.PixelId = settings.PixelId?.Trim();

        return await EditAsync(settings, context);
    }
}
