using System.Security.Claims;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;
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
        var contentType = ((ListGraphType)context.FieldDefinition.ResolvedType!).ResolvedType!.Name;
        var user = _httpContextAccessor.HttpContext?.User;

        if (await _authorizationService.AuthorizeAsync(user, CommonPermissions.ViewContent))
        {
            // No additional check when the user has permission to view all contents
            return query;
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentType);

        if (await AuthorizeDynamicPermissionAsync(user, CommonPermissions.ViewContent, contentTypeDefinition))
        {
            // User has access to view any content item of the given type.
            return query;
        }

        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (await AuthorizeDynamicPermissionAsync(user, CommonPermissions.ViewOwnContent, contentTypeDefinition, userId))
        {
            return query.With<ContentItemIndex>(x => x.ContentType == contentType && x.Owner == userId);
        }

        // Since the user has no permission to this content type, return a query that returns no record.
        return query.With<ContentItemIndex>(x => true == false);
    }

    public override async Task<IEnumerable<ContentItem>> PostQueryAsync(IEnumerable<ContentItem> contentItems, IResolveFieldContext context)
    {
        var filtered = new List<ContentItem>();
        var user = _httpContextAccessor.HttpContext?.User;

        // The only way to ensure no improper disclosure with certainty is post-query filtering each result with the
        // authorization service. Ideally, pre-query filters should have already done all the work by this point so this
        // is just fall-back insurance.
        foreach (var item in contentItems)
        {
            if (await _authorizationService.AuthorizeAsync(user, CommonPermissions.ViewContent, item))
            {
                filtered.Add(item);
            }
        }

        return filtered;
    }

    private Task<bool> AuthorizeDynamicPermissionAsync(
        ClaimsPrincipal user,
        Permission basePermission,
        ContentTypeDefinition contentTypeDefinition,
        string userId = null)
    {
        var template = ContentTypePermissionsHelper.ConvertToDynamicPermission(basePermission);
        var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(template, contentTypeDefinition);

        return _authorizationService.AuthorizeContentTypeAsync(user, dynamicPermission, contentTypeDefinition, userId);
    }
}
