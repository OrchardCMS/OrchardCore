using System;
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

        public override IDisplayResult Edit(LoginSettings settings)
        {
            return Initialize<LoginSettings>("LoginSettings_Edit", model =>
            {
                model.UseSiteTheme = settings.UseSiteTheme;
                model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
                model.DisableLocalLogin = settings.DisableLocalLogin;
                model.UseScriptToSyncRoles = settings.UseScriptToSyncRoles;
                model.SyncRolesScript = settings.SyncRolesScript;
                model.AllowChangingEmail = settings.AllowChangingEmail;
                model.AllowChangingUsername = settings.AllowChangingUsername;
                model.AllowChangingPhoneNumber = settings.AllowChangingPhoneNumber;
            }).Location("Content:5#General")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ManageUsers))
            .OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LoginSettings section, BuildEditorContext context)
        {
            if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase)
                || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
            {
                return null;
            }

            await context.Updater.TryUpdateModelAsync(section, Prefix);

            return await EditAsync(section, context);
        }
    }
}
