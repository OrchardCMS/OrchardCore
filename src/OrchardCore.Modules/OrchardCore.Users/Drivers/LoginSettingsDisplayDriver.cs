using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class LoginSettingsDisplayDriver : SectionDisplayDriver<ISite, LoginSettings>
    {
        public const string GroupId = "LoginSettings";

        public override IDisplayResult Edit(LoginSettings section)
        {
            return Initialize<LoginSettings>("LoginSettings_Edit", model =>
            {
                model.UseSiteTheme = section.UseSiteTheme;
                model.UseExternalProviderIfOnlyOneDefined = section.UseExternalProviderIfOnlyOneDefined;
                model.DisableLocalLogin = section.DisableLocalLogin;
                model.UseScriptToSyncRoles = section.UseScriptToSyncRoles;
                model.SyncRolesScript = section.SyncRolesScript;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LoginSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }
            return Edit(section);
        }
    }
}
