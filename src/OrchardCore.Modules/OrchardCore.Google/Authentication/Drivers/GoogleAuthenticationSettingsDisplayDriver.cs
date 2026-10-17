using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Drivers;

public sealed class GoogleAuthenticationSettingsDisplayDriver : SiteDisplayDriver<GoogleAuthenticationSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    public GoogleAuthenticationSettingsDisplayDriver(
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
        => GoogleConstants.Features.GoogleAuthentication;

    public override async Task<IDisplayResult> EditAsync(ISite site, GoogleAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
        {
            return null;
        }

        return Initialize<GoogleAuthenticationSettingsViewModel>("GoogleAuthenticationSettings_Edit", model =>
        {
            model.ClientID = settings.ClientID;
            model.ClientSecret = SecretInputViewModel.Create(settings.ClientSecret, settings.ClientSecretSecretName);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
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

        var clientSecret = await model.ClientSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication),
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.ClientSecret)}")
        {
            ProtectedValue = settings.ClientSecret,
            SecretName = settings.ClientSecretSecretName,
        });

        if (clientSecret.Succeeded)
        {
            settings.ClientSecret = clientSecret.ProtectedValue;
            settings.ClientSecretSecretName = clientSecret.SecretName;
        }

        settings.ClientID = model.ClientID;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
