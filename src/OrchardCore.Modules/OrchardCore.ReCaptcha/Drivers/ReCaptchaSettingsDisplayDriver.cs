using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Drivers
{
    public class ReCaptchaSettingsDisplayDriver : SectionDisplayDriver<ISite, ReCaptchaSettings>
    {
        public const string GroupId = "recaptcha";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ReCaptchaSettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(ReCaptchaSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
            {
                return null;
            }

            return Initialize<ReCaptchaSettingsViewModel>("ReCaptchaSettings_Edit", model =>
                {
                    model.SiteKey = settings.SiteKey;
                    model.SecretKey = settings.SecretKey;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReCaptchaSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new ReCaptchaSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    section.SiteKey = model.SiteKey?.Trim();
                    section.SecretKey = model.SecretKey?.Trim();

                    // Release the tenant to apply settings.
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}
