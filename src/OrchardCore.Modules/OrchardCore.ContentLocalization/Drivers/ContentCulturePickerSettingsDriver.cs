using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Drivers
{
    public class ContentCulturePickerSettingsDriver : SectionDisplayDriver<ISite, ContentCulturePickerSettings>
    {
        public const string GroupId = "ContentCulturePicker";

        public override IDisplayResult Edit(ContentCulturePickerSettings section)
        {
            return Initialize<ContentCulturePickerSettings>("ContentCulturePickerSettings_Edit", model =>
            {
                model.SetCookie = section.SetCookie;
                model.RedirectToHomepage = section.RedirectToHomepage;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentCulturePickerSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }
            return await EditAsync(section, context);
        }
    }
}
