using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Services;
using OrchardCore.CustomSettings.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Drivers
{
    /// <summary>
    /// This driver generates an editor for site settings. The GroupId represents the type of 
    /// the settings to use.
    /// </summary>
    public class CustomSettingsDisplayDriver : DisplayDriver<ISite>
    {
        private readonly CustomSettingsService _customSettingsService;
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        private readonly Lazy<IList<ContentTypeDefinition>> _contentTypeDefinitions;

        public CustomSettingsDisplayDriver(
            CustomSettingsService customSettingsService, 
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _customSettingsService = customSettingsService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentTypeDefinitions = new Lazy<IList<ContentTypeDefinition>>(() => _customSettingsService.GetSettingsTypes());
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            JToken property;

            var contentTypeDefinition = _contentTypeDefinitions.Value.FirstOrDefault(x => x.Name == context.GroupId);

            if (contentTypeDefinition == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            ContentItem contentItem;

            if (!site.Properties.TryGetValue(contentTypeDefinition.Name, out property))
            {
                contentItem = _contentManager.New(contentTypeDefinition.Name);
            }
            else
            {
                // create existing content item
                contentItem = property.ToObject<ContentItem>();
            }

            var shape = Shape<CustomSettingsEditViewModel>("CustomSettings", async ctx =>
            {
                ctx.Editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, context.Updater);
            }).Location("Content:3").OnGroup(contentTypeDefinition.Name);

            return Task.FromResult<IDisplayResult>(shape);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            JToken property;

            var contentTypeDefinition = _contentTypeDefinitions.Value.FirstOrDefault(x => x.Name == context.GroupId);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            ContentItem contentItem;

            if (!site.Properties.TryGetValue(contentTypeDefinition.Name, out property))
            {
                contentItem = _contentManager.New(contentTypeDefinition.Name);
            }
            else
            {
                // create existing content item
                contentItem = property.ToObject<ContentItem>();
            }

            await _contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater);

            site.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem);

            return await EditAsync(site, context);
        }
    }
}
