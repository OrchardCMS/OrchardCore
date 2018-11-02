using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Forms.Configuration;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Forms.Drivers
{
    public class NoCaptchaSettingsDisplay : SectionDisplayDriver<ISite, NoCaptchaSettings>
    {
        public const string GroupId = "nocaptcha";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public NoCaptchaSettingsDisplay(IShellHost shellHost, ShellSettings shellSettings)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(NoCaptchaSettings section, BuildEditorContext context)
        {
            return Initialize<NoCaptchaSettingsViewModel>("NoCaptchaSettings_Edit", model =>
                {
                    model.SiteKey = section.SiteKey;
                    model.SiteSecret = section.SiteSecret;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(NoCaptchaSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new NoCaptchaSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {

                    section.SiteKey = model.SiteKey?.Trim();
                    section.SiteSecret = model.SiteSecret?.Trim();

                    // Reload tenant to apply settings.
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}
