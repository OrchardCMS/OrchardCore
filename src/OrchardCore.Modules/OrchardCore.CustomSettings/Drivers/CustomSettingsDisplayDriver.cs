using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public CustomSettingsDisplayDriver(
            CustomSettingsService customSettingsService,
            IContentItemDisplayManager contentItemDisplayManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _customSettingsService = customSettingsService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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

            var shape = Initialize<CustomSettingsEditViewModel>(CustomSettingsConstants.Stereotype, async ctx =>
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

            site.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem, _jsonSerializerOptions);

            return await EditAsync(site, context);
        }
    }
}
