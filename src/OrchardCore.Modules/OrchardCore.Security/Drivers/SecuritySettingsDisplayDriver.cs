using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
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

    protected override string SettingsGroupId
        => GroupId;

    public SecuritySettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsSnapshot<SecuritySettings> securitySettings)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _securitySettings = securitySettings.Value;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SecuritySettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<SecuritySettingsViewModel>("SecurityHeadersSettings_Edit", model =>
        {
            // Set the settings from configuration when AdminSettings are overridden via ConfigureSecuritySettings()
            var currentSettings = settings;
            if (_securitySettings.FromConfiguration)
            {
                currentSettings = _securitySettings;
            }

            model.FromConfiguration = currentSettings.FromConfiguration;
            model.ContentSecurityPolicy = currentSettings.ContentSecurityPolicy;
            model.PermissionsPolicy = currentSettings.PermissionsPolicy;
            model.ReferrerPolicy = currentSettings.ReferrerPolicy;

            model.EnableSandbox = currentSettings.ContentSecurityPolicy != null &&
                currentSettings.ContentSecurityPolicy.ContainsKey(ContentSecurityPolicyValue.Sandbox);

            model.UpgradeInsecureRequests = currentSettings.ContentSecurityPolicy != null &&
                currentSettings.ContentSecurityPolicy.ContainsKey(ContentSecurityPolicyValue.UpgradeInsecureRequests);
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
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
