using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.ReCaptchaV3.Configuration;
using OrchardCore.ReCaptchaV3.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptchaV3.Drivers
{
    public class ReCaptchaV3SettingsDisplayDriver : SectionDisplayDriver<ISite, ReCaptchaV3Settings>
    {
        public const string GroupId = "recaptchaV3";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ReCaptchaV3SettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(ReCaptchaV3Settings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaV3Settings))
            {
                return null;
            }

            return Initialize<ReCaptchaV3SettingsViewModel>("ReCaptchaV3Settings_Edit", model =>
                {
                    model.SiteKey = settings.SiteKey;
                    model.SecretKey = settings.SecretKey;
                    model.Threshold = settings.Threshold;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReCaptchaV3Settings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaV3Settings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new ReCaptchaV3SettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    section.SiteKey = model.SiteKey?.Trim();
                    section.SecretKey = model.SecretKey?.Trim();
                    section.Threshold = model.Threshold;

                    // Release the tenant to apply settings.
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}
