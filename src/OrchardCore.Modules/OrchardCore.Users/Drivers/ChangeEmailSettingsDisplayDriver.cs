using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailSettingsDisplayDriver : SectionDisplayDriver<ISite, ChangeEmailSettings>
    {
        public const string GroupId = "ChangeEmailSettings";

        public override IDisplayResult Edit(ChangeEmailSettings section)
        {
            return Initialize<ChangeEmailSettings>("ChangeEmailSettings_Edit", model =>
            {
                model.AllowChangeEmail = section.AllowChangeEmail;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ChangeEmailSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }
            return Edit(section);
        }
    }
}
