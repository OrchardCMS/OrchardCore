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
    public class SecurityHeadersSettingsDisplayDriver : SectionDisplayDriver<ISite, SecurityHeadersOptions>
    {
        internal const string SettingsGroupId = "SecurityHeaders";

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public SecurityHeadersSettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(SecurityHeadersOptions settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.SecurityHeadersSettings))
            {
                return null;
            }

            return Initialize<SecurityHeadersOptions>("SecurityHeadersSettings_Edit", model =>
            {
                model.ReferrerPolicy = settings.ReferrerPolicy;
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SecurityHeadersOptions section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, SecurityPermissions.SecurityHeadersSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var model = new SecurityHeadersOptions();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.ReferrerPolicy = model.ReferrerPolicy;

                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}
