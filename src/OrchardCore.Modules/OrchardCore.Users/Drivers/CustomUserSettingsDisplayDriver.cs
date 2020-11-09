using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Users.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class CustomUserSettingsDisplayDriver : DisplayDriver<User>
    {
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public CustomUserSettingsDisplayDriver(
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            var results = new List<IDisplayResult>();

            foreach(var contentTypeDefinition in GetContentTypeDefinitions())
            {
                var isNew = false;
                var contentItem = await GetUserSettingsAsync(user, contentTypeDefinition, () => isNew = true);
                // this is going to need a prefix.

                var shape = Initialize<CustomUserSettingsEditViewModel>("CustomUserSettings", async model =>
                {
                    model.ContentItem = contentItem;
                    model.Editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, context.Updater, isNew);
                }).Location($"Content:3");//#{contentTypeDefinition.Name}
                results.Add(shape);
            }

            return Combine(results.ToArray());
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {

            foreach(var contentTypeDefinition in GetContentTypeDefinitions())
            {
                var isNew = false;
                var contentItem = await GetUserSettingsAsync(user, contentTypeDefinition, () => isNew = true);
                await _contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, isNew);
                user.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem);
            }

            return await EditAsync(user, context);
        }

        private IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions()
            => _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.GetSettings<ContentTypeSettings>().Stereotype == "CustomUserSettings");

        public async Task<ContentItem> GetUserSettingsAsync(User user, ContentTypeDefinition settingsType, Action isNew = null)
        {
            JToken property;
            ContentItem contentItem;

            if (user.Properties.TryGetValue(settingsType.Name, out property))
            {
                // Create existing content item
                contentItem = property.ToObject<ContentItem>();
            }
            else
            {
                contentItem = await _contentManager.NewAsync(settingsType.Name);
                isNew?.Invoke();
            }

            return contentItem;
        }
    }
}
