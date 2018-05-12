using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.Password")]
    public class PasswordSettingsDisplayDriver : SectionDisplayDriver<ISite, PasswordSettings>
    {
        public const string GroupId = "PasswordSettings";

        public override IDisplayResult Edit(PasswordSettings section)
        {
            return Initialize<PasswordSettings>("PasswordSettings_Edit", model => {
                model.EnableLostPassword = section.EnableLostPassword;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(PasswordSettings section, IUpdateModel updater, string groupId)
        {
            if (groupId == GroupId)
            {
                await updater.TryUpdateModelAsync(section, Prefix);
            }
            return Edit(section);
        }
    }
}
