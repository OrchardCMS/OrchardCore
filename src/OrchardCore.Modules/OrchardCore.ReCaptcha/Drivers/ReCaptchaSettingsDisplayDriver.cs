using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Drivers;

public sealed class ReCaptchaSettingsDisplayDriver : SiteDisplayDriver<ReCaptchaSettings>
{
    public const string GroupId = "recaptcha";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public ReCaptchaSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ReCaptchaSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ReCaptchaPermissions.ManageReCaptchaSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<ReCaptchaSettingsViewModel>("ReCaptchaSettings_Edit", model =>
        {
            model.SiteKey = settings.SiteKey;
            model.SecretKey = settings.SecretKey;
        }).Location("Content")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ReCaptchaSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ReCaptchaPermissions.ManageReCaptchaSettings))
        {
            return null;
        }

        var model = new ReCaptchaSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var protector = _dataProtectionProvider.CreateProtector(ReCaptchaSettingsConfiguration.ProtectorName);

        if (!string.IsNullOrWhiteSpace(model.SecretKey))
        {
            if (settings.SecretKey != model.SecretKey)
            {
                settings.SecretKey = protector.Protect(model.SecretKey);
            }
        }

        settings.SiteKey = model.SiteKey.Trim();

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
