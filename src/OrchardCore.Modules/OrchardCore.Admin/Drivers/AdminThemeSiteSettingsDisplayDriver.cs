using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Drivers
{
    public class AdminThemeSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, AdminThemeSettings>
    {
        public const string GroupId = "adminTheme";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AdminThemeSiteSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AdminThemeSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminTheme.ManageAdminTheme))
            {
                return null;
            }

            return Initialize<AdminThemeSettingsViewModel>("AdminThemeSettings_Edit", model =>
                {
                    model.Theme = String.Join(", ", settings.Theme);
                }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(AdminThemeSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminTheme.ManageAdminTheme))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new AdminThemeSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Theme = model.Theme;
            }

            return await EditAsync(settings, context);
        }
    }
}
