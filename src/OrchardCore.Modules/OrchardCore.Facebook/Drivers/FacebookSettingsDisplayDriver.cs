using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public sealed class FacebookSettingsDisplayDriver : SiteDisplayDriver<FacebookSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FacebookSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => FacebookConstants.Features.Core;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<FacebookSettingsViewModel>("FacebookSettings_Edit", model =>
        {
            model.AppId = settings.AppId;
            model.FBInit = settings.FBInit;
            model.FBInitParams = settings.FBInitParams;
            model.Version = settings.Version;
            model.SdkJs = settings.SdkJs;
            model.AppSecretSecretName = settings.AppSecretSecretName;
            model.HasAppSecret = !string.IsNullOrWhiteSpace(settings.AppSecret);
        }).Location("Content:0")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        var model = new FacebookSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.AppId = model.AppId;
        settings.FBInit = model.FBInit;
        settings.SdkJs = model.SdkJs;
        settings.Version = model.Version;
        settings.AppSecretSecretName = model.AppSecretSecretName;

        if (!string.IsNullOrWhiteSpace(model.FBInitParams))
        {
            settings.FBInitParams = model.FBInitParams;
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
