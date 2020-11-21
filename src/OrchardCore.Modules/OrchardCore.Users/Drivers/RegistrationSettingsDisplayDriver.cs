using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    [Feature("OrchardCore.Users.Registration")]
    public class RegistrationSettingsDisplayDriver : SectionDisplayDriver<ISite, RegistrationSettings>
    {
        public const string GroupId = "RegistrationSettings";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public RegistrationSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }
        public override async Task<IDisplayResult> EditAsync(RegistrationSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageUsers))
            {
                return null;
            }

            return Initialize<RegistrationSettings>("RegistrationSettings_Edit", model =>
            {
                model.UsersCanRegister = section.UsersCanRegister;
                model.UsersMustValidateEmail = section.UsersMustValidateEmail;
                model.UseSiteTheme = section.UseSiteTheme;
                model.NoPasswordForExternalUsers = section.NoPasswordForExternalUsers;
                model.NoUsernameForExternalUsers = section.NoUsernameForExternalUsers;
                model.NoEmailForExternalUsers = section.NoEmailForExternalUsers;
                model.UseScriptToGenerateUsername = section.UseScriptToGenerateUsername;
                model.GenerateUsernameScript = section.GenerateUsernameScript;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(RegistrationSettings section, BuildEditorContext context)
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
