using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Forms.Drivers
{
    public class ReCaptchaSettingsDisplay : SectionDisplayDriver<ISite, ReCaptchaSettings>
    {
        public const string GroupId = "recaptcha";

        public override IDisplayResult Edit(ReCaptchaSettings section, BuildEditorContext context)
        {
            return Initialize<ReCaptchaSettingsViewModel>("ReCaptchaSettings_Edit", model =>
                {
                    model.SiteKey = section.SiteKey;
                    model.SiteSecret = section.SiteSecret;
                })
                .Location("Content")
                .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReCaptchaSettings section, IUpdateModel updater, string groupId)
        {
            if (groupId == GroupId)
            {
                var model = new ReCaptchaSettingsViewModel();

                if (await updater.TryUpdateModelAsync(model, Prefix))
                {

                    section.SiteKey = model.SiteKey?.Trim();
                    section.SiteSecret = model.SiteSecret?.Trim();
                }
            }

            return Edit(section);
        }
    }
}
