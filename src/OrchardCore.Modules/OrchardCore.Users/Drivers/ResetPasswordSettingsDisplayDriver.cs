using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.ResetPassword")]
    public class ResetPasswordSettingsDisplayDriver : SectionDisplayDriver<ISite, ResetPasswordSettings>
    {
        public const string GroupId = "ResetPasswordSettings";

        public override IDisplayResult Edit(ResetPasswordSettings section)
        {
            return Initialize<ResetPasswordSettings>("ResetPasswordSettings_Edit", model => {
                model.AllowResetPassword = section.AllowResetPassword;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ResetPasswordSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }
            return await EditAsync(section, context);
        }
    }
}
