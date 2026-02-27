using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.ViewModels;

namespace OrchardCore.Twitter.Drivers;

public sealed class TwitterSettingsDisplayDriver : SiteDisplayDriver<TwitterSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TwitterSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => TwitterConstants.Features.Twitter;

    public override async Task<IDisplayResult> EditAsync(ISite site, TwitterSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<TwitterSettingsViewModel>("TwitterSettings_Edit", model =>
        {
            model.APIKey = settings.ConsumerKey;
            model.ConsumerSecretSecretName = settings.ConsumerSecretSecretName;
            model.HasConsumerSecret = !string.IsNullOrWhiteSpace(settings.ConsumerSecret);
            model.AccessToken = settings.AccessToken;
            model.AccessTokenSecretSecretName = settings.AccessTokenSecretSecretName;
            model.HasAccessTokenSecret = !string.IsNullOrWhiteSpace(settings.AccessTokenSecret);
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, TwitterSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitter))
        {
            return null;
        }

        var model = new TwitterSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ConsumerKey = model.APIKey;
        settings.ConsumerSecretSecretName = model.ConsumerSecretSecretName;
        settings.AccessToken = model.AccessToken;
        settings.AccessTokenSecretSecretName = model.AccessTokenSecretSecretName;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
