using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Google.TagManager.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Drivers;

public sealed class GoogleTagManagerSettingsDisplayDriver : SiteDisplayDriver<GoogleTagManagerSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public GoogleTagManagerSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<GoogleTagManagerSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GoogleConstants.Features.GoogleTagManager;

    public override async Task<IDisplayResult> EditAsync(ISite site, GoogleTagManagerSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleTagManager))
        {
            return null;
        }

        return Initialize<GoogleTagManagerSettingsViewModel>("GoogleTagManagerSettings_Edit", model =>
        {
            model.ContainerID = settings.ContainerID;
        }).Location(l => l.Zone("Content", "5").Card(S["Container"], "10"))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, GoogleTagManagerSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleTagManager))
        {
            return null;
        }

        var model = new GoogleTagManagerSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.Updater.ModelState.IsValid)
        {
            settings.ContainerID = model.ContainerID;
        }

        return await EditAsync(site, settings, context);
    }
}
