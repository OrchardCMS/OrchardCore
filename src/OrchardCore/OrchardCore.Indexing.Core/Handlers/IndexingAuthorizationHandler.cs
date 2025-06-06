using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Models;
using OrchardCore.Security;

namespace OrchardCore.Indexing.Core.Handlers;

public sealed class IndexingAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    private IAuthorizationService _authorizationService;

    public IndexingAuthorizationHandler(IServiceProvider serviceProvider)
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

        if (context.Resource is not IndexProfile indexProfile || requirement.Permission != IndexingPermissions.QuerySearchIndex)
        {
            return;
        }

        _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();

        var permission = IndexingPermissions.CreateDynamicPermission(indexProfile);

        if (await _authorizationService.AuthorizeAsync(context.User, permission))
        {
            context.Succeed(requirement);

            return;
        }
    }
}
