using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Drivers;

public sealed class GoogleAuthenticationSettingsDisplayDriver : SiteDisplayDriver<GoogleAuthenticationSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GoogleAuthenticationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => GoogleConstants.Features.GoogleAuthentication;

    public override async Task<IDisplayResult> EditAsync(ISite site, GoogleAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<GoogleAuthenticationSettingsViewModel>("GoogleAuthenticationSettings_Edit", model =>
        {
            model.ClientID = settings.ClientID;
            model.ClientSecretSecretName = settings.ClientSecretSecretName;
            model.HasClientSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, GoogleAuthenticationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
        {
            return null;
        }

        var model = new GoogleAuthenticationSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ClientID = model.ClientID;
        settings.ClientSecretSecretName = model.ClientSecretSecretName;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
