using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class RegistrationSettingsDisplayDriver : SectionDisplayDriver<ISite, RegistrationSettings>
    {
        public const string GroupId = "registrationSettings";

        public override IDisplayResult Edit(RegistrationSettings section)
        {
            return Shape<RegistrationSettings>("RegistrationSettings_Edit", model => {
                model.UsersCanRegister = section.UsersCanRegister;
                model.EnableLostPassword = section.EnableLostPassword;

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
