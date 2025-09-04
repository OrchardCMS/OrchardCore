using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class CustomUserSettingsDisplayDriver : DisplayDriver<User>
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

    public override async Task<IDisplayResult> EditAsync(User user, BuildEditorContext context)
    {
        var contentTypeDefinitions = await GetContentTypeDefinitionsAsync();
        if (!contentTypeDefinitions.Any())
        {
            return null;
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

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
    {
        var userClaim = _httpContextAccessor.HttpContext.User;
        var contentTypeDefinitions = await GetContentTypeDefinitionsAsync();

        foreach (var contentTypeDefinition in contentTypeDefinitions)
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

    private async Task<IEnumerable<ContentTypeDefinition>> GetContentTypeDefinitionsAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(x => x.GetStereotype() == "CustomUserSettings");

    private async Task<ContentItem> GetUserSettingsAsync(User user, ContentTypeDefinition settingsType, Action isNew = null)
    {
        JsonNode property;
        ContentItem contentItem;

        if (user.Properties.TryGetPropertyValue(settingsType.Name, out property))
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
