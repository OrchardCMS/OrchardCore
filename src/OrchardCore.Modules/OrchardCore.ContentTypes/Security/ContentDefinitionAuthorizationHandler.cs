using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes.Security;
public class ContentDefinitionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private IAuthorizationService _authorizationService;

    public ContentDefinitionAuthorizationHandler(IServiceProvider serviceProvider)
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

        // If we are not evaluating a specific content definition, return.
        if (context.Resource == null)
        {
            return;
        }

        Permission permission = null;

        if (context.Resource is ContentTypeDefinition contentTypeDefinition)
        {
            permission = Permissions.CreatePermissionForType(contentTypeDefinition);
        }
        else if (context.Resource is ContentPartDefinition contentPartDefinition)
        {
            permission = Permissions.CreatePermissionForPart(contentPartDefinition);
        }
        else if (requirement.Permission.Name.Equals(Permissions.EditContentTypes.Name, StringComparison.OrdinalIgnoreCase))
        {
            permission = requirement.Permission;
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
