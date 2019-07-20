using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Services
{
    public class CustomSettingsService
    {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private IDictionary<string, ContentTypeDefinition> _settingsTypes;

        public CustomSettingsService(
            ISiteService siteService,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefinitionManager)
        {
            _siteService = siteService;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<IEnumerable<string>> GetAllSettingsTypeNamesAsync()
        {
            await EnsureSettingsTypesAsync();
            return _settingsTypes.Keys;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> GetAllSettingsTypesAsync()
        {
            await EnsureSettingsTypesAsync();
            return _settingsTypes.Values;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> GetSettingsTypesAsync(params string[] settingsTypeNames)
        {
            await EnsureSettingsTypesAsync();

            var settingsTypes = new List<ContentTypeDefinition>();
            foreach (var settingsTypeName in settingsTypeNames)
            {
                ContentTypeDefinition settingsType;
                if (_settingsTypes.TryGetValue(settingsTypeName, out settingsType))
                {
                    settingsTypes.Add(settingsType);
                }
            }

            return settingsTypes;
        }

        public async Task<ContentTypeDefinition> GetSettingsTypeAsync(string settingsTypeName)
        {
            await EnsureSettingsTypesAsync();

            _settingsTypes.TryGetValue(settingsTypeName, out var settingsType);

            return settingsType;
        }

        private async Task EnsureSettingsTypesAsync()
        {
            if (_settingsTypes == null)
            {
                _settingsTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                     .Where(x => x.Settings.ToObject<ContentTypeSettings>().Stereotype == "CustomSettings")
                     .ToDictionary(x => x.Name);
            }
        }

        public async Task<bool> CanUserCreateSettingsAsync(ContentTypeDefinition settingsType)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return await _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(settingsType));
        }

        public async Task<ContentItem> GetSettingsAsync(string settingsTypeName, Action isNew = null)
        {
            var settingsType = await GetSettingsTypeAsync(settingsTypeName);
            if (settingsType == null)
            {
                return null;
            }

            return await GetSettingsAsync(settingsType, isNew);
        }

        public async Task<ContentItem> GetSettingsAsync(ContentTypeDefinition settingsType, Action isNew = null)
        {
            var site = await _siteService.GetSiteSettingsAsync();

            return await GetSettingsAsync(site, settingsType, isNew);
        }

        public async Task<ContentItem> GetSettingsAsync(ISite site, ContentTypeDefinition settingsType, Action isNew = null)
        {
            JToken property;
            ContentItem contentItem;

            if (site.Properties.TryGetValue(settingsType.Name, out property))
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
