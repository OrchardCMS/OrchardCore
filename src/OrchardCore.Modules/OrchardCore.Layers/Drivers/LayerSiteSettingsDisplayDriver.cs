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
    public class LayerSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, LayerSettings>
    {
        public const string GroupId = "zones";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public LayerSiteSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(LayerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLayers))
            {
                return null;
            }

            return Initialize<LayerSettingsViewModel>("LayerSettings_Edit", model =>
                {
                    model.Zones = String.Join(", ", settings.Zones);
                }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(LayerSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageLayers))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new LayerSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Zones = (model.Zones ?? String.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return await EditAsync(settings, context);
        }
    }
}
