using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings.Services;

public class CustomSettingsService
{
    private readonly ISiteService _siteService;
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly Lazy<Task<IDictionary<string, ContentTypeDefinition>>> _settingsTypes;

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
        _settingsTypes = new Lazy<Task<IDictionary<string, ContentTypeDefinition>>>(GetContentTypeAsync);
    }

    public async Task<IEnumerable<string>> GetAllSettingsTypeNamesAsync()
        => (await _settingsTypes.Value).Keys;

    public async Task<IEnumerable<ContentTypeDefinition>> GetAllSettingsTypesAsync()
        => (await _settingsTypes.Value).Values;

    public async Task<IEnumerable<ContentTypeDefinition>> GetSettingsTypesAsync(params string[] settingsTypeNames)
    {
        var types = await _settingsTypes.Value;
        var definitions = new List<ContentTypeDefinition>();

        foreach (var settingsTypeName in settingsTypeNames)
        {
            ContentTypeDefinition settingsType;
            if (types.TryGetValue(settingsTypeName, out settingsType))
            {
                definitions.Add(settingsType);
            }
        }

        return definitions;
    }

    public ContentTypeDefinition GetSettingsType(string settingsTypeName)
        => GetSettingsTypeAsync(settingsTypeName).Result;

    public async Task<ContentTypeDefinition> GetSettingsTypeAsync(string settingsTypeName)
    {
        (await _settingsTypes.Value).TryGetValue(settingsTypeName, out var settingsType);

        return settingsType;
    }

    public Task<bool> CanUserCreateSettingsAsync(ContentTypeDefinition settingsType)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return _authorizationService.AuthorizeAsync(user, Permissions.CreatePermissionForType(settingsType));
    }

    public Task<ContentItem> GetSettingsAsync(string settingsTypeName, Action isNew = null)
    {
        var settingsType = GetSettingsType(settingsTypeName);
        if (settingsType == null)
        {
            return Task.FromResult<ContentItem>(null);
        }

        return GetSettingsAsync(settingsType, isNew);
    }

    public async Task<ContentItem> GetSettingsAsync(ContentTypeDefinition settingsType, Action isNew = null)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        return await GetSettingsAsync(site, settingsType, isNew);
    }

    public async Task<ContentItem> GetSettingsAsync(ISite site, ContentTypeDefinition settingsType, Action isNew = null)
    {
        JsonNode property;
        ContentItem contentItem;

        if (site.Properties.TryGetPropertyValue(settingsType.Name, out property))
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

    private async Task<IDictionary<string, ContentTypeDefinition>> GetContentTypeAsync()
    {
        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        var result = contentTypes.Where(x => x.StereotypeEquals(CustomSettingsConstants.Stereotype))
        .ToDictionary(x => x.Name);

        return result;
    }
}
