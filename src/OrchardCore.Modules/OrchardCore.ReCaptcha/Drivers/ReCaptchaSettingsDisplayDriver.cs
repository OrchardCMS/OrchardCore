using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReCaptcha.Drivers
{
    public class ReCaptchaSettingsDisplayDriver : SectionDisplayDriver<ISite, ReCaptchaSettings>
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

        public override async Task<IDisplayResult> EditAsync(ReCaptchaSettings settings, BuildEditorContext context)
        {
            if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
            {
                return null;
            }

            context.Shape.Metadata.Wrappers.Add("Settings_Wrapper__Reload");

            return Initialize<ReCaptchaSettingsViewModel>("ReCaptchaSettings_Edit", model =>
                {
                    model.SiteKey = settings.SiteKey;
                    model.SecretKey = settings.SecretKey;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReCaptchaSettings settings, UpdateEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReCaptchaSettings))
            {
                return null;
            }

            if (context.GroupId.EqualsOrdinalIgnoreCase(GroupId))
            {
                var model = new ReCaptchaSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.SiteKey = model.SiteKey?.Trim();
                settings.SecretKey = model.SecretKey?.Trim();

                _shellReleaseManager.RequestRelease();
            }

            return await EditAsync(settings, context);
        }
    }
}
