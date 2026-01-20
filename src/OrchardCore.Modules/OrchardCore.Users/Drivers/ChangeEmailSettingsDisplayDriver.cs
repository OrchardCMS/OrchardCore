using System;
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
    [Feature("OrchardCore.Users.ChangeEmail")]
    public class ChangeEmailSettingsDisplayDriver : SectionDisplayDriver<ISite, ChangeEmailSettings>
    {
        public const string GroupId = "userChangeEmail";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ChangeEmailSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }
        public override async Task<IDisplayResult> EditAsync(ChangeEmailSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
            {
                return null;
            }

            return Initialize<ChangeEmailSettings>("ChangeEmailSettings_Edit", model =>
            {
                model.AllowChangeEmail = settings.AllowChangeEmail;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ChangeEmailSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.ManageUsers))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }

            return await EditAsync(section, context);
        }
    }
}
