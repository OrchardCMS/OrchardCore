using System.Threading.Tasks;
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

namespace OrchardCore.Security.Drivers
{
    public class SecuritySettingsDisplayDriver : SectionDisplayDriver<ISite, SecuritySettings>
    {
        internal const string SettingsGroupId = "SecurityHeaders";

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly SecuritySettings _securitySettings;

        public SecuritySettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IOptionsSnapshot<SecuritySettings> securitySettings)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _securitySettings = securitySettings.Value;
        }

        public override async Task<IDisplayResult> EditAsync(SecuritySettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
            {
                return null;
            }

            return Initialize<SecuritySettingsViewModel>("SecurityHeadersSettings_Edit", model =>
            {
                // Set the settings from configuration when AdminSettings are overriden via ConfigureSecuritySettings()
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
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SecuritySettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var model = new SecuritySettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                PrepareContentSecurityPolicyValues(model);

                section.ContentTypeOptions = SecurityHeaderDefaults.ContentTypeOptions;
                section.ContentSecurityPolicy = model.ContentSecurityPolicy;
                section.PermissionsPolicy = model.PermissionsPolicy;
                section.ReferrerPolicy = model.ReferrerPolicy;

                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
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
}
