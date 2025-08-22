using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Options;
using OrchardCore.Security.Settings;
using OrchardCore.Security.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Security.Drivers;

public sealed class SecuritySettingsDisplayDriver : SiteDisplayDriver<SecuritySettings>
{
    internal const string GroupId = "SecurityHeaders";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly SecuritySettings _securitySettings;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    protected override string SettingsGroupId
        => GroupId;

    public SecuritySettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsSnapshot<SecuritySettings> securitySettings,
        INotifier notifier,
        IHtmlLocalizer<SecuritySettingsDisplayDriver> htmlLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _securitySettings = securitySettings.Value;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SecuritySettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        // Set the settings from configuration when AdminSettings are overridden via ConfigureSecuritySettings()
        var currentSettings = settings;
        if (_securitySettings.FromConfiguration)
        {
            currentSettings = _securitySettings;

            await _notifier.InformationAsync(H["The current settings are coming from configuration sources, saving the settings will affect the AdminSettings not the configuration."]);
        }

        var contentSecurityPolicyShapeResult = Initialize<SecuritySettingsViewModel>("ContentSecurityPolicySettings_Edit", model =>
        {
            model.FromConfiguration = currentSettings.FromConfiguration;

            model.ContentSecurityPolicy = settings.ContentSecurityPolicy;

            model.EnableSandbox = currentSettings.ContentSecurityPolicy != null &&
                currentSettings.ContentSecurityPolicy.ContainsKey(ContentSecurityPolicyValue.Sandbox);

            model.UpgradeInsecureRequests = currentSettings.ContentSecurityPolicy != null &&
                currentSettings.ContentSecurityPolicy.ContainsKey(ContentSecurityPolicyValue.UpgradeInsecureRequests);
        }).Location("Content:2#Content Security Policy;5")
        .OnGroup(SettingsGroupId);

        var permissionsPolicyShapeResult = Initialize<SecuritySettingsViewModel>("PermissionsPolicySettings_Edit", model
            => model.PermissionsPolicy = currentSettings.PermissionsPolicy)
            .Location("Content:2#Permissions Policy;10")
            .OnGroup(SettingsGroupId);

        var referrerPolicyShapeResult = Initialize<SecuritySettingsViewModel>("ReferrerPolicySettings_Edit", model
            => model.ReferrerPolicy = currentSettings.ReferrerPolicy)
            .Location("Content:2#Referrer Policy;15")
            .OnGroup(SettingsGroupId);

        return Combine(contentSecurityPolicyShapeResult, permissionsPolicyShapeResult, referrerPolicyShapeResult);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SecuritySettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
        {
            return null;
        }

        var model = new SecuritySettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        PrepareContentSecurityPolicyValues(model);

        settings.ContentTypeOptions = SecurityHeaderDefaults.ContentTypeOptions;
        settings.ContentSecurityPolicy = model.ContentSecurityPolicy;
        settings.PermissionsPolicy = model.PermissionsPolicy;
        settings.ReferrerPolicy = model.ReferrerPolicy;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }

    private static void PrepareContentSecurityPolicyValues(SecuritySettingsViewModel model)
    {
        if (!model.EnableSandbox)
        {
            model.ContentSecurityPolicy.Remove(ContentSecurityPolicyValue.Sandbox);
        }
        else if (!model.ContentSecurityPolicy.TryGetValue(ContentSecurityPolicyValue.Sandbox, out _))
        {
            model.ContentSecurityPolicy[ContentSecurityPolicyValue.Sandbox] = null;
        }

        if (!model.UpgradeInsecureRequests)
        {
            model.ContentSecurityPolicy.Remove(ContentSecurityPolicyValue.UpgradeInsecureRequests);
        }
        else
        {
            model.ContentSecurityPolicy[ContentSecurityPolicyValue.UpgradeInsecureRequests] = null;
        }
    }
}
