using System.Threading.Tasks;
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

        public ReCaptchaSettingsDisplayDriver(IShellHost shellHost, ShellSettings shellSettings)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(ReCaptchaSettings section, BuildEditorContext context)
        {
            return Initialize<ReCaptchaSettingsViewModel>("ReCaptchaSettings_Edit", model =>
                {
                    model.SiteKey = section.SiteKey;
                    model.SecretKey = section.SecretKey;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReCaptchaSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
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
