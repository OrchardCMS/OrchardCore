using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public class ContentItemFilters : GraphQLFilter<ContentItem>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;

    public ContentItemFilters(IHttpContextAccessor httpContextAccessor,
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

        if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ViewContent))
        {
            // No additinal check when the user has permission to view all contents
            return query;
        }

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
        var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewContent);
        var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentTypeDefinition);

        if (await _authorizationService.AuthorizeContentTypeAsync(_httpContextAccessor.HttpContext.User, dynamicPermission, contentTypeDefinition))
        {
            // User has access to view any content item of the given type.
            return query;
        }

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var contentTypeOwnPermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(CommonPermissions.ViewOwnContent);

        if (await _authorizationService.AuthorizeContentTypeAsync(_httpContextAccessor.HttpContext.User, contentTypeOwnPermission, contentTypeDefinition, userId))
        {
            return query.With<ContentItemIndex>(x => x.ContentType == contentType && x.Owner == userId);
        }

        // Since the user has no permission to this content type, return a query that returns no record.
        return query.With<ContentItemIndex>(x => true == false);
    }
}
