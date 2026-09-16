using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Drivers;

public sealed class CustomUserSettingsDisplayDriver : DisplayDriver<User>
{
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly CustomUserSettingsService _settingsService;
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
        _settingsService = new CustomUserSettingsService(contentManager, contentDefinitionManager, session: null);
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> DisplayAsync(User user, BuildDisplayContext context)
    {
        if (!string.Equals(context.DisplayType, OrchardCoreConstants.DisplayType.SummaryAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var contentTypeDefinitions = await GetContentTypeDefinitionsAsync();
        if (!contentTypeDefinitions.Any())
        {
            return null;
        }

        var results = new List<IDisplayResult>();
        var userClaim = _httpContextAccessor.HttpContext.User;

        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            results.Add(Factory("CustomUserSettings_SummaryAdmin", async builder =>
                {
                    var contentItem = await GetUserSettingsAsync(user, contentTypeDefinition);
                    return await _contentItemDisplayManager.BuildDisplayAsync(contentItem, updater: null, OrchardCoreConstants.DisplayType.SummaryAdmin);
                })
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, PlacementInfo.HiddenLocation)
                .Differentiator($"CustomUserSettings-{contentTypeDefinition.Name}")
                .RenderWhen(() => _authorizationService.AuthorizeAsync(userClaim, CustomUserSettingsPermissions.CreatePermissionForType(contentTypeDefinition))));
        }

        return Combine(results);
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
            CustomUserSettingsService.SetSettings(user, contentTypeDefinition, contentItem);
        }

        return await EditAsync(user, context);
    }

    private Task<IEnumerable<ContentTypeDefinition>> GetContentTypeDefinitionsAsync()
        => _settingsService.GetAllSettingsTypesAsync();

    private Task<ContentItem> GetUserSettingsAsync(User user, ContentTypeDefinition settingsType, Action isNew = null)
        => _settingsService.GetSettingsAsync(user, settingsType, () =>
        {
            isNew?.Invoke();
            return Task.CompletedTask;
        });
}
