using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class LoginSettingsDisplayDriver : SectionDisplayDriver<ISite, LoginSettings>
    {
        public const string GroupId = "userLogin";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public LoginSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }
        public override async Task<IDisplayResult> EditAsync(LoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageUsers))
            {
                return null;
            }

            return Initialize<LoginSettings>("LoginSettings_Edit", model =>
            {
                model.UseSiteTheme = settings.UseSiteTheme;
                model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
                model.DisableLocalLogin = settings.DisableLocalLogin;
                model.UseScriptToSyncRoles = settings.UseScriptToSyncRoles;
                model.SyncRolesScript = settings.SyncRolesScript;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LoginSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageUsers))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }

            return await EditAsync(section, context);
        }
    }
}
