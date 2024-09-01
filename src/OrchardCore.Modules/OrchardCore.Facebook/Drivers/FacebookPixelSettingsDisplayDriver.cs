using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public sealed class FacebookPixelSettingsDisplayDriver : SiteDisplayDriver<FacebookPixelSettings>
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

    protected override string SettingsGroupId
        => FacebookConstants.PixelSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookPixelSettings settings, BuildEditorContext context)
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
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookPixelSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageFacebookApp))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        settings.PixelId = settings.PixelId?.Trim();

        return await EditAsync(site, settings, context);
    }
}
