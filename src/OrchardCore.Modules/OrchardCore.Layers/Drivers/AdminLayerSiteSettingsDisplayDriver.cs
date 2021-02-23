using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Drivers
{
    public class AdminLayerSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, AdminLayerSettings>
    {
        public const string GroupId = "adminzones";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AdminLayerSiteSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AdminLayerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, AdminLayerPermissions.ManageAdminLayers))
            {
                return null;
            }

            return Initialize<LayerSettingsViewModel>("LayerSettings_Edit", model =>
                {
                    model.Zones = String.Join(", ", settings.Zones);
                }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(AdminLayerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, AdminLayerPermissions.ManageAdminLayers))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new LayerSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Zones = (model.Zones ?? String.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return await EditAsync(settings, context);
        }
    }
}
