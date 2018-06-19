using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata.Models;
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
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly Lazy<IList<ContentTypeDefinition>> _contentTypeDefinitions;

        public CustomSettingsDisplayDriver(
            CustomSettingsService customSettingsService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _customSettingsService = customSettingsService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _contentTypeDefinitions = new Lazy<IList<ContentTypeDefinition>>(() => _customSettingsService.GetSettingsTypes());
        }

        public override async Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            JToken property;

            var contentTypeDefinition = _contentTypeDefinitions.Value.FirstOrDefault(x => x.Name == context.GroupId);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(contentTypeDefinition)))
            {
                return null;
            }            

            ContentItem contentItem;
            bool isNew;

            if (!site.Properties.TryGetValue(contentTypeDefinition.Name, out property))
            {
                contentItem = await _contentManager.NewAsync(contentTypeDefinition.Name);
                isNew = true;
            }
            else
            {
                // Create existing content item
                contentItem = property.ToObject<ContentItem>();
                isNew = false;
            }

            var shape = Initialize<CustomSettingsEditViewModel>("CustomSettings", async ctx =>
            {
                ctx.Editor = await _contentItemDisplayManager.BuildEditorAsync(contentItem, context.Updater, isNew);
            }).Location("Content:3").OnGroup(contentTypeDefinition.Name);

            return shape;
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            JToken property;

            var contentTypeDefinition = _contentTypeDefinitions.Value.FirstOrDefault(x => x.Name == context.GroupId);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(contentTypeDefinition)))
            {
                return null;
            }

            ContentItem contentItem;
            bool isNew;

            if (!site.Properties.TryGetValue(contentTypeDefinition.Name, out property))
            {
                contentItem = await _contentManager.NewAsync(contentTypeDefinition.Name);
                isNew = true;
            }
            else
            {
                // Create existing content item
                contentItem = property.ToObject<ContentItem>();
                isNew = false;
            }

            await _contentItemDisplayManager.UpdateEditorAsync(contentItem, context.Updater, isNew);

            site.Properties[contentTypeDefinition.Name] = JObject.FromObject(contentItem);

            return await EditAsync(site, context);
        }
    }
}
