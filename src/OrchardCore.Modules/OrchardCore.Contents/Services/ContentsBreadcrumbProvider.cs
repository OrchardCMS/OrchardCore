using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes;
using OrchardCore.Contents.Controllers;
using OrchardCore.Localization.Data;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Describes the breadcrumb trails of the content management screens.
/// </summary>
public sealed class ContentsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Contents" },
        { "contentTypeId", string.Empty },
    };

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDataLocalizer D;

    internal readonly IStringLocalizer S;

    public ContentsBreadcrumbProvider(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IDataLocalizer dataLocalizer,
        IStringLocalizer<ContentsBreadcrumbProvider> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        D = dataLocalizer;
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
        => builder.Name switch
        {
            ContentsBreadcrumbs.List => BuildListAsync(builder),
            ContentsBreadcrumbs.Create => BuildEditorAsync(builder, isNew: true),
            ContentsBreadcrumbs.Edit => BuildEditorAsync(builder, isNew: false),
            _ => ValueTask.CompletedTask,
        };

    private async ValueTask BuildListAsync(BreadcrumbBuilder builder)
    {
        await AddListAsync(builder);

        var contentType = builder.GetData<string>(ContentsBreadcrumbs.ContentTypeData);

        if (string.IsNullOrEmpty(contentType))
        {
            return;
        }

        // The list is filtered on a single content type, which becomes the node of the page.
        var typeDisplayName = await GetTypeDisplayNameAsync(contentType);

        builder.Add(typeDisplayName, item => item.Id("ContentType"));
    }

    private async ValueTask BuildEditorAsync(BreadcrumbBuilder builder, bool isNew)
    {
        if (!builder.TryGetData<ContentItem>(ContentsBreadcrumbs.ContentItemData, out var contentItem))
        {
            return;
        }

        await AddListAsync(builder);

        var typeDisplayName = await GetTypeDisplayNameAsync(contentItem.ContentType);

        builder.Add(isNew ? S["New {0}", typeDisplayName] : S["Edit {0}", typeDisplayName],
            item => item.Id("ContentItem"));
    }

    private async ValueTask AddListAsync(BreadcrumbBuilder builder)
    {
        // The node is rendered as plain text when the user can't reach the list, so that the trail stays complete.
        var canList = await CanListContentAsync();

        builder.Add(S["Manage Content"], item =>
        {
            item.Id("Contents");

            if (canList)
            {
                item.Action(nameof(AdminController.List), typeof(AdminController).ControllerName(), s_listRouteValues);
            }
        });
    }

    private async ValueTask<bool> CanListContentAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            return false;
        }

        var listableTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(definition => definition.IsListable());

        return await _authorizationService.AuthorizeContentTypeDefinitionsAsync(user, CommonPermissions.ListContent, listableTypes, _contentManager);
    }

    private async ValueTask<string> GetTypeDisplayNameAsync(string contentType)
    {
        var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentType);
        var displayName = definition?.DisplayName ?? contentType.CamelFriendly();

        return D[displayName, DataLocalizationContext.ContentType];
    }
}
