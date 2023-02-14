using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{
    public class CustomUserSettingsDisplayDriver : DisplayDriver<User>
    {
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomUserSettingsDisplayDriver(
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
        {
            var contentTypeDefinitions = GetContentTypeDefinitions();
            if (!contentTypeDefinitions.Any())
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            var results = new List<IDisplayResult>();
            var userClaim = _httpContextAccessor.HttpContext.User;

            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                results.Add(Initialize<CustomUserSettingsEditViewModel>("CustomUserSettings", async model =>
                    {
                        var isNew = false;
                        var contentItem = await GetUserSettingsAsync(user, contentTypeDefinition, () => isNew = true);
                        model.Editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, context.Updater, isNew, context.GroupId, Prefix);
                    })
                    .Location($"Content:10#{contentTypeDefinition.DisplayName}")
                    .Differentiator($"CustomUserSettings-{contentTypeDefinition.Name}")
                    .RenderWhen(() => _authorizationService.AuthorizeAsync(userClaim, CustomUserSettingsPermissions.CreatePermissionForType(contentTypeDefinition))));
            }

            return Task.FromResult<IDisplayResult>(Combine(results.ToArray()));
        }

        public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            var userClaim = _httpContextAccessor.HttpContext.User;
            foreach (var contentTypeDefinition in GetContentTypeDefinitions())
            {
                if (!await _authorizationService.AuthorizeAsync(userClaim, CustomUserSettingsPermissions.CreatePermissionForType(contentTypeDefinition)))
                {
                    continue;
                }

                var isNew = false;
                var contentItem = await GetUserSettingsAsync(user, contentTypeDefinition, () => isNew = true);
                await _contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, isNew, context.GroupId, Prefix);
                user.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem);
            }

            return await EditAsync(user, context);
        }

        private IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions()
            => _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(x => x.GetStereotype() == "CustomUserSettings");

        private async Task<ContentItem> GetUserSettingsAsync(User user, ContentTypeDefinition settingsType, Action isNew = null)
        {
            JToken property;
            ContentItem contentItem;

            if (user.Properties.TryGetValue(settingsType.Name, out property))
            {
                var existing = property.ToObject<ContentItem>();

                // Create a new item to take into account the current type definition.
                contentItem = await _contentManager.NewAsync(existing.ContentType);
                contentItem.Merge(existing);
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
