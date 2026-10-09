using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public sealed class FacebookSettingsDisplayDriver : SiteDisplayDriver<FacebookSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    public FacebookSettingsDisplayDriver(
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
        => FacebookConstants.Features.Core;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        return Initialize<FacebookSettingsViewModel>("FacebookSettings_Edit", model =>
        {
            model.AppId = settings.AppId;
            model.AppSecret = SecretInputViewModel.Create(settings.AppSecret, settings.AppSecretSecretName);
            model.FBInit = settings.FBInit;
            model.FBInitParams = settings.FBInitParams;
            model.Version = settings.Version;
            model.SdkJs = settings.SdkJs;
        }).Location("Content:0")
        .OnGroup(SettingsGroupId);
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

        var appSecret = await model.AppSecret.UpdateAsync(new SecretInputUpdateContext(
            _serviceProvider,
            _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core),
            context.Updater.ModelState,
            $"{Prefix}.{nameof(model.AppSecret)}")
        {
            ProtectedValue = settings.AppSecret,
            SecretName = settings.AppSecretSecretName,
            Description = "Meta app secret",
        });

        if (appSecret.Succeeded)
        {
            settings.AppSecret = appSecret.ProtectedValue;
            settings.AppSecretSecretName = appSecret.SecretName;
        }

        settings.AppId = model.AppId;
        settings.FBInit = model.FBInit;
        settings.SdkJs = model.SdkJs;
        settings.Version = model.Version;

        if (!string.IsNullOrWhiteSpace(model.FBInitParams))
        {
            settings.FBInitParams = model.FBInitParams;
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
