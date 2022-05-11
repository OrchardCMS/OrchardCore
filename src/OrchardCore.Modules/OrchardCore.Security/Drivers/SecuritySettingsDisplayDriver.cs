using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Options;
using OrchardCore.Security.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Security.Drivers
{
    public class SecuritySettingsDisplayDriver : SectionDisplayDriver<ISite, SecurityHeadersOptions>
    {
        internal const string SettingsGroupId = "SecurityHeaders";

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly SecurityHeadersOptions _securityHeadersOptions;

        public SecuritySettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IOptionsSnapshot<SecurityHeadersOptions> securityHeadersOptions)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _securityHeadersOptions = securityHeadersOptions.Value;
        }

        public override async Task<IDisplayResult> EditAsync(SecurityHeadersOptions settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.ManageSecurityHeadersSettings))
            {
                return null;
            }

            return Initialize<SecuritySettingsViewModel>("SecurityHeadersSettings_Edit", model =>
            {
                model.ContentSecurityPolicy = _securityHeadersOptions.ContentSecurityPolicy;
                model.ContentSecurityPolicyValues = SecurityHeaderDefaults.ContentSecurityPolicyNames.ToList();
                model.FrameOptions = _securityHeadersOptions.FrameOptions;
                model.PermissionsPolicy = _securityHeadersOptions.PermissionsPolicy;
                model.PermissionsPolicyValues = SecurityHeaderDefaults.PermissionsPolicyNames.ToList();
                model.ReferrerPolicy = _securityHeadersOptions.ReferrerPolicy;
                model.EnableSandbox = model.ContentSecurityPolicy != null && model.ContentSecurityPolicy.Any(p => p.StartsWith(ContentSecurityPolicyValue.Sandbox));
                model.UpgradeInsecureRequests = model.ContentSecurityPolicy != null && model.ContentSecurityPolicy.Any(p => p == ContentSecurityPolicyValue.UpgradeInsecureRequests);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SecurityHeadersOptions section, BuildEditorContext context)
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

                model.PermissionsPolicyValues.RemoveAll(p => SecurityHeaderDefaults.PermissionsPolicyNames.Contains(p));

                section.ContentTypeOptions = SecurityHeaderDefaults.ContentTypeOptions;
                section.ContentSecurityPolicy = model.ContentSecurityPolicyValues.ToArray();
                section.FrameOptions = model.FrameOptions;
                section.PermissionsPolicy = model.PermissionsPolicyValues.ToArray();
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
            var sandboxPolicy = model.ContentSecurityPolicyValues.SingleOrDefault(p => p.StartsWith(ContentSecurityPolicyValue.Sandbox));
            var hasSandboxPolicyWithoutValues = sandboxPolicy == ContentSecurityPolicyValue.Sandbox;
            var upgradeInsecureRequestsPolicy = model.ContentSecurityPolicyValues.SingleOrDefault(p => p == ContentSecurityPolicyValue.UpgradeInsecureRequests);

            model.ContentSecurityPolicyValues.RemoveAll(p => SecurityHeaderDefaults.ContentSecurityPolicyNames.Contains(p));

            if (model.EnableSandbox && hasSandboxPolicyWithoutValues)
            {
                model.ContentSecurityPolicyValues.Add(ContentSecurityPolicyValue.Sandbox);
            }

            if (!model.EnableSandbox && sandboxPolicy != null)
            {
                model.ContentSecurityPolicyValues.Remove(sandboxPolicy);
            }

            if (model.UpgradeInsecureRequests)
            {
                model.ContentSecurityPolicyValues.Add(ContentSecurityPolicyValue.UpgradeInsecureRequests);
            }

            if (!model.UpgradeInsecureRequests && upgradeInsecureRequestsPolicy != null)
            {
                model.ContentSecurityPolicyValues.Remove(ContentSecurityPolicyValue.UpgradeInsecureRequests);
            }
        }
    }
}
