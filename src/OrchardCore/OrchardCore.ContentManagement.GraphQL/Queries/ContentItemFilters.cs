using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using System.Security.Claims;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public sealed class ContentItemFilters : GraphQLFilter<ContentItem>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;

    public ContentItemFilters(
        IHttpContextAccessor httpContextAccessor,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
    }

    public override async Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, IResolveFieldContext context)
    {
        var contentType = ((ListGraphType)(context.FieldDefinition).ResolvedType).ResolvedType.Name;

        if (_httpContextAccessor.HttpContext is not { User: { } user })
        {
            // Since the user is not available, return a query that returns no record.
            return query.With<ContentItemIndex>(x => true == false);
        }

        if (await _authorizationService.AuthorizeAsync(user, CommonPermissions.ViewContent))
        {
            // No additional check when the user has permission to view all contents
            return query;
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentType);
        var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewContent);
        var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentTypeDefinition);

        if (await _authorizationService.AuthorizeContentTypeAsync(user, dynamicPermission, contentTypeDefinition))
        {
            // User has access to view any content item of the given type.
            return query;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var contentTypeOwnPermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewOwnContent);

        if (await _authorizationService.AuthorizeContentTypeAsync(user, contentTypeOwnPermission, contentTypeDefinition, userId))
        {
            return query.With<ContentItemIndex>(x => x.ContentType == contentType && x.Owner == userId);
        }

        // Since the user has no permission to this content type, return a query that returns no record.
        return query.With<ContentItemIndex>(x => true == false);
    }

    public override async Task<IEnumerable<ContentItem>> PostQueryAsync(IEnumerable<ContentItem> contentItems, IResolveFieldContext context)
    {
        if (_httpContextAccessor.HttpContext is not { User: { } user })
        {
            return [];
        }

        var results = new List<ContentItem>();
        foreach (var item in contentItems)
        {
            if (await _authorizationService.AuthorizeAsync(user, CommonPermissions.ViewContent, item))
            {
                results.Add(item);
            }
        }

        return results;
    }
}
