using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Display;
using OrchardCore.CustomSettings.Services;
using OrchardCore.CustomSettings.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Drivers
{
    /// <summary>
    /// This driver generates an editor for site settings. The GroupId represents the type of
    /// the settings to use.
    /// </summary>
    public class CustomSettingsDisplayDriver : DisplayDriver<ISite>
    {
        private readonly CustomSettingsService _customSettingsService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        public CustomSettingsDisplayDriver(
            CustomSettingsService customSettingsService,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _customSettingsService = customSettingsService;
            _contentItemDisplayManager = contentItemDisplayManager;
        }

        public override async Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            var contentTypeDefinition = _customSettingsService.GetSettingsType(context.GroupId);
            if (contentTypeDefinition == null)
            {
                return null;
            }

            if (!await _customSettingsService.CanUserCreateSettingsAsync(contentTypeDefinition))
            {
                return null;
            }

            var isNew = false;
            var contentItem = await _customSettingsService.GetSettingsAsync(site, contentTypeDefinition, () => isNew = true);

            var shape = Initialize<CustomSettingsEditViewModel>("CustomSettings", async ctx =>
            {
                ctx.Editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, context.Updater, isNew);
            }).Location("Content:3").OnGroup(contentTypeDefinition.Name);

            return shape;
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            var contentTypeDefinition = _customSettingsService.GetSettingsType(context.GroupId);
            if (contentTypeDefinition == null)
            {
                return null;
            }

            if (!await _customSettingsService.CanUserCreateSettingsAsync(contentTypeDefinition))
            {
                return null;
            }

            var isNew = false;
            var contentItem = await _customSettingsService.GetSettingsAsync(site, contentTypeDefinition, () => isNew = true);

            await _contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, isNew);

            site.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem);

            return await EditAsync(site, context);
        }
    }
}
