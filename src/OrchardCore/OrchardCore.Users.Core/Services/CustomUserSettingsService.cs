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
        => (await _settingsTypes.Value.ConfigureAwait(false)).Keys;

    public async Task<IEnumerable<ContentTypeDefinition>> GetAllSettingsTypesAsync()
        => (await _settingsTypes.Value.ConfigureAwait(false)).Values;

    public async Task<IEnumerable<ContentTypeDefinition>> GetSettingsTypesAsync(params string[] settingsTypeNames)
    {
        var types = await _settingsTypes.Value.ConfigureAwait(false);
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
        var types = await _settingsTypes.Value.ConfigureAwait(false);

        types.TryGetValue(settingsTypeName, out var settingsType);
        return settingsType;
    }

    public async Task<Dictionary<string, ContentItem>> GetSettingsAsync(string settingsTypeName, Func<Task> factoryAsync = null)
    {
        var settingsType = await GetSettingsTypeAsync(settingsTypeName).ConfigureAwait(false);
        if (settingsType == null)
        {
            return [];
        }

        return await GetSettingsAsync(settingsType, factoryAsync).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, ContentItem>> GetSettingsAsync(ContentTypeDefinition settingsType, Func<Task> factoryAsync = null)
    {
        var users = await _session.Query<User>().ListAsync().ConfigureAwait(false);
        var contentItems = new Dictionary<string, ContentItem>();
        foreach (var user in users)
        {
            var item = await GetSettingsAsync(user, settingsType, factoryAsync).ConfigureAwait(false);
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

        if (user.Properties.TryGetPropertyValue(settingsType.Name, out var property))
        {
            var existing = property.ToObject<ContentItem>();

            // Create a new item to take into account the current type definition.
            contentItem = await _contentManager.NewAsync(existing.ContentType).ConfigureAwait(false);
            contentItem.Merge(existing);

            return contentItem;
        }

        contentItem = await _contentManager.NewAsync(settingsType.Name).ConfigureAwait(false);

        if (factoryAsync != null)
        {
            await factoryAsync.Invoke().ConfigureAwait(false);
        }

        return contentItem;
    }

    private async Task<IDictionary<string, ContentTypeDefinition>> GetContentTypeAsync()
    {
        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync().ConfigureAwait(false);

        var result = contentTypes.Where(x => x.StereotypeEquals("CustomUserSettings"))
        .ToDictionary(x => x.Name);

        return result;
    }
}
