using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services;

public class CustomUserSettingsService
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly Lazy<IDictionary<string, ContentTypeDefinition>> _settingsTypes;
    private readonly YesSql.ISession _session;

    public CustomUserSettingsService(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        ISession session)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _settingsTypes = new Lazy<IDictionary<string, ContentTypeDefinition>>(
            () => _contentDefinitionManager
            .ListTypeDefinitions()
            .Where(x => x.StereotypeEquals("CustomUserSettings"))
            .ToDictionary(x => x.Name));

        _session = session;
    }

    public IEnumerable<string> GetAllSettingsTypeNames() => _settingsTypes.Value.Keys;

    public IEnumerable<ContentTypeDefinition> GetAllSettingsTypes()
    {
        return _settingsTypes.Value.Values;
    }

    public IEnumerable<ContentTypeDefinition> GetSettingsTypes(params string[] settingsTypeNames)
    {
        foreach (var settingsTypeName in settingsTypeNames)
        {
            if (_settingsTypes.Value.TryGetValue(settingsTypeName, out ContentTypeDefinition settingsType))
            {
                yield return settingsType;
            }
        }
    }

    public ContentTypeDefinition GetSettingsType(string settingsTypeName)
    {
        _settingsTypes.Value.TryGetValue(settingsTypeName, out ContentTypeDefinition settingsType);
        return settingsType;
    }

    public Task<Dictionary<string, ContentItem>> GetSettingsAsync(string settingsTypeName, Func<Task> factoryAsync = null)
    {
        var settingsType = GetSettingsType(settingsTypeName);
        if (settingsType == null)
        {
            return Task.FromResult<Dictionary<string, ContentItem>>(null);
        }

        return GetSettingsAsync(settingsType, factoryAsync);
    }

    public async Task<Dictionary<string, ContentItem>> GetSettingsAsync(ContentTypeDefinition settingsType, Func<Task> factoryAsync = null)
    {
        var users = await _session.Query<User>().ListAsync();
        var contentItems = new Dictionary<string, ContentItem>();
        foreach (var user in users)
        {
            var item = await GetSettingsAsync(user, settingsType, factoryAsync);
            if (item != null)
            {
                contentItems.Add(user.UserId, item);
            }
        }

        return contentItems;
    }

    public async Task<ContentItem> GetSettingsAsync(User user, ContentTypeDefinition settingsType, Func<Task> factoryAsync = null)
    {
        ContentItem contentItem;

        if (user.Properties.TryGetValue(settingsType.Name, out JToken property))
        {
            var existing = property.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            contentItem = await _contentManager.NewAsync(existing.ContentType);
            contentItem.Merge(existing);

            return contentItem;
        }

        contentItem = await _contentManager.NewAsync(settingsType.Name);
        await factoryAsync?.Invoke();

        return contentItem;
    }
}
