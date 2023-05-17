using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Security.Services;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class LoginSettingsDisplayDriver : SectionDisplayDriver<ISite, LoginSettings>
    {
        public const string GroupId = "userLogin";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;

        public LoginSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IStringLocalizer<LoginSettingsDisplayDriver> stringLocalizer,
            IRoleService roleService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            S = stringLocalizer;
        }
        public override async Task<IDisplayResult> EditAsync(LoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
            {
                return null;
            }

            var contentResult = Initialize<LoginSettings>("LoginSettings_Edit", model =>
            {
                model.UseSiteTheme = settings.UseSiteTheme;
                model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
                model.DisableLocalLogin = settings.DisableLocalLogin;
                model.UseScriptToSyncRoles = settings.UseScriptToSyncRoles;
                model.SyncRolesScript = settings.SyncRolesScript;
                model.AllowChangingEmail = settings.AllowChangingEmail;
                model.AllowChangingUsername = settings.AllowChangingUsername;
            }).Location("Content:5#General")
            .OnGroup(GroupId);

            var enableTwoFaResult = Initialize<LoginSettings>("LoginSettingsEnableTwoFactorAuthentication_Edit", model =>
            {
                model.EnableTwoFactorAuthentication = settings.EnableTwoFactorAuthentication;
            }).Location("Content:5#Two Factor Authentication")
            .OnGroup(GroupId);

            var twofaResult = Initialize<LoginSettings>("LoginSettingsTwoFactorAuthentication_Edit", model =>
            {
                model.EnableTwoFactorAuthentication = settings.EnableTwoFactorAuthentication;
                model.NumberOfRecoveryCodesToGenerate = settings.NumberOfRecoveryCodesToGenerate;
                model.UseEmailAsAuthenticatorDisplayName = settings.UseEmailAsAuthenticatorDisplayName;
                model.RequireTwoFactorAuthentication = settings.RequireTwoFactorAuthentication;
                model.AllowRememberClientTwoFactorAuthentication = settings.AllowRememberClientTwoFactorAuthentication;
                model.TokenLength = settings.TokenLength;
            }).Location("Content:10#Two Factor Authentication")
            .OnGroup(GroupId);

            return Combine(contentResult, enableTwoFaResult, twofaResult);
        }

        public override async Task<IDisplayResult> UpdateAsync(LoginSettings section, BuildEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);

                if (section.NumberOfRecoveryCodesToGenerate < 1)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(section.NumberOfRecoveryCodesToGenerate), S["Number of Recovery Codes to Generate should be grater than 0."]);
                }

                if (section.TokenLength != 6 && section.TokenLength != 8)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(section.TokenLength), S["The token length should be either 6 or 8."]);
                }

                if (context.Updater.ModelState.IsValid)
                {
                    section.UseSiteTheme = section.UseSiteTheme;
                    section.UseExternalProviderIfOnlyOneDefined = section.UseExternalProviderIfOnlyOneDefined;
                    section.DisableLocalLogin = section.DisableLocalLogin;
                    section.UseScriptToSyncRoles = section.UseScriptToSyncRoles;
                    section.SyncRolesScript = section.SyncRolesScript;
                    section.AllowChangingEmail = section.AllowChangingEmail;
                    section.AllowChangingUsername = section.AllowChangingUsername;
                    section.EnableTwoFactorAuthentication = section.EnableTwoFactorAuthentication;
                    section.NumberOfRecoveryCodesToGenerate = section.NumberOfRecoveryCodesToGenerate;
                    section.UseEmailAsAuthenticatorDisplayName = section.UseEmailAsAuthenticatorDisplayName;
                    section.RequireTwoFactorAuthentication = section.RequireTwoFactorAuthentication;
                    section.AllowRememberClientTwoFactorAuthentication = section.AllowRememberClientTwoFactorAuthentication;
                    section.TokenLength = section.TokenLength;
                }
            }

            return await EditAsync(section, context);
        }
    }
}
