using Microsoft.AspNetCore.Authorization;
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

    public ReCaptchaSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ReCaptchaSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
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

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
        {
            return null;
        }

        var model = new ReCaptchaSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.SiteKey = model.SiteKey?.Trim();
        settings.SecretKey = model.SecretKey?.Trim();

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
