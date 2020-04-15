using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.ThemeSettings.Models;
using OrchardCore.ThemeSettings.ViewModels;

namespace OrchardCore.ThemeSettings.Drivers
{
    public class ThemeSettingsDisplayDriver : SectionDisplayDriver<ISite, CustomThemeSettings>
    {
        public const string GroupId = "CustomTheme";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ThemeSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(CustomThemeSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageThemeSettings))
            {
                return null;
            }

            return Initialize<ThemeSettingsViewModel>("ThemeSettings_Edit", model =>
            {
                model.Head = settings.Head ?? string.Empty;
                model.Foot = settings.Foot ?? string.Empty;
            }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomThemeSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageThemeSettings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new ThemeSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Head = model.Head;
                settings.Foot = model.Foot;
            }

            return await EditAsync(settings, context);
        }
    }
}
