using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class RoleAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    private IAuthorizationService _authorizationService;
    private IRoleService _roleService;

    public RoleAuthorizationHandler(IServiceProvider serviceProvider)
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

        if (context.Resource == null)
        {
            return;
        }

        // Lazy load to prevent circular dependencies
        _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();
        _roleService ??= _serviceProvider?.GetService<IRoleService>();

        if (context.Resource is IRole role)
        {
            var variantPermission = GetPermissionVariation(requirement.Permission, role.RoleName);

            if (variantPermission != null && await _authorizationService.AuthorizeAsync(context.User, variantPermission).ConfigureAwait(false))
            {
                context.Succeed(requirement);

                return;
            }
        }

        if (context.Resource is User user)
        {
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (requirement.Permission.Name == UsersPermissions.EditUsers.Name
                && user.UserId == currentUserId
                && await _authorizationService.AuthorizeAsync(context.User, UsersPermissions.EditOwnUser).ConfigureAwait(false))
            {
                context.Succeed(requirement);

                return;
            }

            if (await _authorizationService.AuthorizeAsync(context.User, requirement.Permission).ConfigureAwait(false))
            {
                context.Succeed(requirement);

                return;
            }

            IEnumerable<string> roleNames = user.RoleNames ?? [];

            if (!roleNames.Any())
            {
                // When the user is in no roles, we check to see if the current user can manage any roles.
                roleNames = (await _roleService.GetAssignableRolesAsync().ConfigureAwait(false))
                    .Select(x => x.RoleName);
            }

            // Check every role to see if the current user has permission to at least one role.
            foreach (var roleName in roleNames)
            {
                var variantPermission = GetPermissionVariation(requirement.Permission, roleName);

                if (variantPermission != null && await _authorizationService.AuthorizeAsync(context.User, variantPermission).ConfigureAwait(false))
                {
                    context.Succeed(requirement);

                    return;
                }
            }
        }
    }

    private static Permission GetPermissionVariation(Permission permission, string roleName)
    {
        if (permission.Name == UsersPermissions.ListUsers.Name)
        {
            return UsersPermissions.CreateListUsersInRolePermission(roleName);
        }

        if (permission.Name == UsersPermissions.EditUsers.Name)
        {
            return UsersPermissions.CreateEditUsersInRolePermission(roleName);
        }

        if (permission.Name == UsersPermissions.DeleteUsers.Name)
        {
            return UsersPermissions.CreateDeleteUsersInRolePermission(roleName);
        }

        if (permission.Name == UsersPermissions.AssignRoleToUsers.Name)
        {
            return UsersPermissions.CreateAssignRoleToUsersPermission(roleName);
        }

        if (permission.Name == UsersPermissions.ManageUsers.Name)
        {
            return UsersPermissions.CreatePermissionForManageUsersInRole(roleName);
        }

        return null;
    }
}
