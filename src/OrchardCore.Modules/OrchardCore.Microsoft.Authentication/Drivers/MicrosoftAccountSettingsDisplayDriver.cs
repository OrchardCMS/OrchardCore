using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers;

public sealed class MicrosoftAccountSettingsDisplayDriver : SiteDisplayDriver<MicrosoftAccountSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    public MicrosoftAccountSettingsDisplayDriver(
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
        => MicrosoftAuthenticationConstants.Features.MicrosoftAccount;

    public override async Task<IDisplayResult> EditAsync(ISite site, MicrosoftAccountSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        return Initialize<MicrosoftAccountSettingsViewModel>("MicrosoftAccountSettings_Edit", model =>
        {
            model.AppId = settings.AppId;
            model.AppSecret = SecretInputViewModel.Create(settings.AppSecret, settings.AppSecretSecretName);
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
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

        var appSecret = await model.AppSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount),
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.AppSecret)}")
        {
            ProtectedValue = settings.AppSecret,
            SecretName = settings.AppSecretSecretName,
        });

        if (appSecret.Succeeded)
        {
            settings.AppSecret = appSecret.ProtectedValue;
            settings.AppSecretSecretName = appSecret.SecretName;
        }

        settings.AppId = model.AppId;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
