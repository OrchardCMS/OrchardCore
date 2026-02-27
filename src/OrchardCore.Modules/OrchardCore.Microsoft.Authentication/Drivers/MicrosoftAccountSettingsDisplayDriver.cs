using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers;

public sealed class MicrosoftAccountSettingsDisplayDriver : SiteDisplayDriver<MicrosoftAccountSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MicrosoftAccountSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => MicrosoftAuthenticationConstants.Features.MicrosoftAccount;

    public override async Task<IDisplayResult> EditAsync(ISite site, MicrosoftAccountSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<MicrosoftAccountSettingsViewModel>("MicrosoftAccountSettings_Edit", model =>
        {
            model.AppId = settings.AppId;
            model.AppSecretSecretName = settings.AppSecretSecretName;
            model.HasAppSecret = !string.IsNullOrWhiteSpace(settings.AppSecret);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, MicrosoftAccountSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        var model = new MicrosoftAccountSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.AppId = model.AppId;
        settings.AppSecretSecretName = model.AppSecretSecretName;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
