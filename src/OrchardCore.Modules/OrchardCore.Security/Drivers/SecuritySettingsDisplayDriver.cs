using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Security.Drivers
{
    public class SecuritySettingsDisplayDriver : SectionDisplayDriver<ISite, SecuritySettings>
    {
        internal const string SettingsGroupId = "SecurityHeaders";

        private static readonly IList<string> _defaultPermissionsPolicy = new List<string>();

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public SecuritySettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
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
                model.FrameOptions = settings.FrameOptions;
                model.PermissionsPolicy = settings.PermissionsPolicy;
                model.PermissionsPolicyOrigin = settings.PermissionsPolicyOrigin;
                model.ReferrerPolicy = settings.ReferrerPolicy;
                model.StrictTransportSecurityMaxAge = settings.StrictTransportSecurity.MaxAge.Days;
                model.StrictTransportSecurityIncludeSubDomains = settings.StrictTransportSecurity.IncludeSubDomains;
                model.StrictTransportSecurityPreload = settings.StrictTransportSecurity.Preload;
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

                section.FrameOptions = model.FrameOptions;
                section.PermissionsPolicy = model.PermissionsPolicy ?? _defaultPermissionsPolicy;
                section.PermissionsPolicyOrigin = model.PermissionsPolicyOrigin;
                section.ReferrerPolicy = model.ReferrerPolicy;
                section.StrictTransportSecurity.MaxAge = TimeSpan.FromDays(model.StrictTransportSecurityMaxAge);
                section.StrictTransportSecurity.IncludeSubDomains = model.StrictTransportSecurityIncludeSubDomains;
                section.StrictTransportSecurity.Preload = model.StrictTransportSecurityPreload;

                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}
