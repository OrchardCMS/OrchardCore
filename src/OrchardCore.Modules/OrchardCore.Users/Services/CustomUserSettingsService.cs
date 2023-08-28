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
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services;
public class CustomUserSettingsService
{
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly Lazy<IDictionary<string, ContentTypeDefinition>> _settingsTypes;
    private readonly YesSql.ISession _session;

    public CustomUserSettingsService(
        ISiteService siteService,
        IContentManager contentManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IContentDefinitionManager contentDefinitionManager,
        YesSql.ISession session)
    {
        _contentManager = contentManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _contentDefinitionManager = contentDefinitionManager;
        _settingsTypes = new Lazy<IDictionary<string, ContentTypeDefinition>>(
            () => _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(x => x.GetStereotype() == "CustomUserSettings")
                    .ToDictionary(x => x.Name));
        _session = session;
    }

    public IEnumerable<string> GetAllSettingsTypeNames()
    {
        return _settingsTypes.Value.Keys;
    }

    public IEnumerable<ContentTypeDefinition> GetAllSettingsTypes()
    {
        return _settingsTypes.Value.Values;
    }

    public IEnumerable<ContentTypeDefinition> GetSettingsTypes(params string[] settingsTypeNames)
    {
        foreach (var settingsTypeName in settingsTypeNames)
        {
            if (_settingsTypes.Value.TryGetValue(settingsTypeName, out ContentTypeDefinition  settingsType))
                yield return settingsType;
            }
        }
    }

    public ContentTypeDefinition GetSettingsType(string settingsTypeName)
    {
        _settingsTypes.Value.TryGetValue(settingsTypeName, out ContentTypeDefinition settingsType);
        return settingsType;
    }

    public async Task<bool> CanUserCreateSettingsAsync(ContentTypeDefinition settingsType)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // todo:
        // return _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(settingsType));
        await Task.Delay(0);
        return true;
    }

    public Task<Dictionary<string, ContentItem>> GetSettingsAsync(string settingsTypeName, Action isNew = null)
    {
        var settingsType = GetSettingsType(settingsTypeName);
        if (settingsType == null)
        {
            return Task.FromResult<Dictionary<string, ContentItem>>(null);
        }

        return GetSettingsAsync(settingsType, isNew);
    }

    public async Task<Dictionary<string, ContentItem>> GetSettingsAsync(ContentTypeDefinition settingsType, Action isNew = null)
    {
        // foreach user get settings
        var users = await _session.Query<User>().ListAsync();
        var contentItems = new Dictionary<string, ContentItem>();
        foreach (var user in users){
            var item = await GetSettingsAsync(user, settingsType, isNew);
            if (item != null)
            {
                contentItems.Add(user.UserId, item);
            }
        }

        return contentItems;
    }

    public async Task<ContentItem> GetSettingsAsync(User user, ContentTypeDefinition settingsType, Action isNew = null)
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
