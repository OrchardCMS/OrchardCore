using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.ViewModels;

namespace OrchardCore.Twitter.Drivers;

public sealed class TwitterSettingsDisplayDriver : SiteDisplayDriver<TwitterSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    public TwitterSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
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

        return Initialize<TwitterSettingsViewModel>("TwitterSettings_Edit", model =>
        {
            model.APIKey = settings.ConsumerKey;
            model.ConsumerSecret = SecretInputViewModel.Create(settings.ConsumerSecret, settings.ConsumerSecretSecretName);
            model.AccessToken = settings.AccessToken;
            model.AccessTokenSecret = SecretInputViewModel.Create(settings.AccessTokenSecret, settings.AccessTokenSecretSecretName);
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
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

        var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

        var consumerSecret = await model.ConsumerSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            protector,
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.ConsumerSecret)}")
        {
            ProtectedValue = settings.ConsumerSecret,
            SecretName = settings.ConsumerSecretSecretName,
            Description = "X (Twitter) API secret key",
        });

        if (consumerSecret.Succeeded)
        {
            settings.ConsumerSecret = consumerSecret.ProtectedValue;
            settings.ConsumerSecretSecretName = consumerSecret.SecretName;
        }

        var accessTokenSecret = await model.AccessTokenSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            protector,
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.AccessTokenSecret)}")
        {
            ProtectedValue = settings.AccessTokenSecret,
            SecretName = settings.AccessTokenSecretSecretName,
            Description = "X (Twitter) access token secret",
        });

        if (accessTokenSecret.Succeeded)
        {
            settings.AccessTokenSecret = accessTokenSecret.ProtectedValue;
            settings.AccessTokenSecretSecretName = accessTokenSecret.SecretName;
        }

        settings.ConsumerKey = model.APIKey;
        settings.AccessToken = model.AccessToken;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
