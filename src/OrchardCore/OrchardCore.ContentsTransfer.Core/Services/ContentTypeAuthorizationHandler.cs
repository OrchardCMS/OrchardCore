using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Security;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentTypeAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private IAuthorizationService _authorizationService;

    public ContentTypeAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded)
        {
            // This handler is not revoking any pre-existing grants.
            return;
        }

        // If we are not evaluating a ContentItem then return.
        if (context.Resource == null)
        {
            return;
        }

        Permission permission = null;

        // The resource can be a content type name
        var contentType = context.Resource is ContentItem contentItem
            ? contentItem.ContentType
            : context.Resource.ToString();

        if (requirement.Permission.Name == ContentTransferPermissions.ImportContentFromFile.Name)
        {
            permission = ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ImportContentFromFileOfType, contentType);
        }
        else if (requirement.Permission.Name == ContentTransferPermissions.ExportContentFromFile.Name)
        {
            permission = ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ExportContentFromFileOfType, contentType);
        }

        if (permission == null)
        {
            return;
        }

        // Lazy load to prevent circular dependencies
        _authorizationService ??= _serviceProvider.GetService<IAuthorizationService>();

        if (await _authorizationService.AuthorizeAsync(context.User, permission))
        {
            context.Succeed(requirement);
        }
    }
}
