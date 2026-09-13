using System.Text.Json.Nodes;
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
    private readonly Lazy<Task<IDictionary<string, ContentTypeDefinition>>> _settingsTypes;
    private readonly ISession _session;

    public CustomUserSettingsService(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        ISession session)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _settingsTypes = new Lazy<Task<IDictionary<string, ContentTypeDefinition>>>(GetContentTypeAsync);
        _session = session;
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
            if (types.TryGetValue(settingsTypeName, out var settingsType))
            {
                definitions.Add(settingsType);
            }
        }

        return definitions;
    }

    public async Task<ContentTypeDefinition> GetSettingsTypeAsync(string settingsTypeName)
    {
        var types = await _settingsTypes.Value;

        types.TryGetValue(settingsTypeName, out var settingsType);
        return settingsType;
    }

    public async Task<Dictionary<string, ContentItem>> GetSettingsAsync(string settingsTypeName, Func<Task> factoryAsync = null)
    {
        var settingsType = await GetSettingsTypeAsync(settingsTypeName);
        if (settingsType == null)
        {
            return [];
        }

        return await GetSettingsAsync(settingsType, factoryAsync);
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

    /// <summary>
    /// Builds a detached settings item using the canonical current type definition and stored user values.
    /// </summary>
    public async Task<ContentItem> GetSettingsAsync(User user, ContentTypeDefinition settingsType, Func<Task> factoryAsync = null)
    {
        ContentItem contentItem;

        if (user.Properties.TryGetPropertyValue(settingsType.Name, out var property))
        {
            var existing = property.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            contentItem = await _contentManager.NewAsync(settingsType.Name);
            contentItem.Merge(existing);
            contentItem.ContentType = settingsType.Name;

            return contentItem;
        }

        contentItem = await _contentManager.NewAsync(settingsType.Name);

        if (factoryAsync != null)
        {
            await factoryAsync.Invoke();
        }

        return contentItem;
    }

    /// <summary>
    /// Stores settings on their owning user without creating a standalone content item.
    /// The caller persists the user through the identity manager.
    /// </summary>
    public static void SetSettings(User user, ContentTypeDefinition settingsType, ContentItem contentItem)
    {
        contentItem.ContentType = settingsType.Name;
        user.Properties[settingsType.Name] = JObject.FromObject(contentItem);
    }

    private async Task<IDictionary<string, ContentTypeDefinition>> GetContentTypeAsync()
    {
        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        var result = contentTypes.Where(x => x.StereotypeEquals("CustomUserSettings"))
        .ToDictionary(x => x.Name);

        return result;
    }
}
