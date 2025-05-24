using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Search.Abstractions;
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

        if (context.Resource is not SearchPermissionParameters parameters || string.IsNullOrEmpty(parameters.IndexName))
        {
            return;
        }

        _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();

        var permission = IndexingPermissions.CreateDynamicPermission(parameters.IndexName);

        if (await _authorizationService.AuthorizeAsync(context.User, permission))
        {
            context.Succeed(requirement);

            return;
        }
    }
}
