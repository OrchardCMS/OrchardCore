using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Media.Drivers;

public sealed class MediaApiSettingsDisplayDriver : SiteDisplayDriver<MediaApiSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => MediaApiSettings.GroupId;

    public MediaApiSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<MediaApiSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, MediaApiSettings settings, BuildEditorContext context)
        => Initialize<MediaApiSettingsViewModel>("MediaApiSettings_Edit", model =>
        {
            model.AuthenticationScheme = settings.AuthenticationScheme;
            model.AuthenticationSchemes =
            [
                new SelectListItem(S["Cookie (default)"], nameof(MediaApiAuthenticationScheme.Cookie)),
                new SelectListItem(S["Bearer — OAuth2 + PKCE (requires OpenID Connect)"], nameof(MediaApiAuthenticationScheme.Bearer)),
            ];
        }).Location("Content:1")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, MediaPermissions.ManageMediaApiSettings))
        .OnGroup(SettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(ISite site, MediaApiSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaApiSettings))
        {
            return null;
        }

        var model = new MediaApiSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (settings.AuthenticationScheme != model.AuthenticationScheme)
        {
            settings.AuthenticationScheme = model.AuthenticationScheme;

            // Rebuild the shell so the "MediaApi" authorization policy is reconfigured for the new scheme.
            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}
