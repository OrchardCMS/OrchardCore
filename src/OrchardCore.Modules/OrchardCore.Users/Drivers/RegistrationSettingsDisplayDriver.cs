using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationSettingsDisplayDriver : SectionDisplayDriver<ISite, RegistrationSettings>
    {
        public const string GroupId = "RegistrationSettings";

        public override IDisplayResult Edit(RegistrationSettings section)
        {
            return Initialize<RegistrationSettings>("RegistrationSettings_Edit", model => {
                model.UsersCanRegister = section.UsersCanRegister;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(RegistrationSettings section, IUpdateModel updater, string groupId)
        {
            if (groupId == GroupId)
            {
                await updater.TryUpdateModelAsync(section, Prefix);
            }
            return Edit(section);
        }
    }
}
