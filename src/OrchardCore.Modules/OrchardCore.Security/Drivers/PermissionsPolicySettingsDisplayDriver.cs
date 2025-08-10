using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Settings;
using OrchardCore.Security.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Security.Drivers;

public sealed class PermissionsPolicySettingsDisplayDriver : SiteDisplayDriver<SecuritySettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly SecuritySettings _securitySettings;

    protected override string SettingsGroupId
        => ContentSecurityPolicySettingsDisplayDriver.GroupId;

    public PermissionsPolicySettingsDisplayDriver(
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

        return Initialize<SecuritySettingsViewModel>("PermissionsPolicySettings_Edit", model =>
        {
            // Set the settings from configuration when AdminSettings are overridden via ConfigureSecuritySettings()
            var currentSettings = settings;
            if (_securitySettings.FromConfiguration)
            {
                currentSettings = _securitySettings;
            }

            model.PermissionsPolicy = currentSettings.PermissionsPolicy;
        }).Location("Content:2#Permissions Policy;10")
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

        settings.PermissionsPolicy = model.PermissionsPolicy;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
