using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class RoleAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
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

            if (variantPermission != null && await _authorizationService.AuthorizeAsync(context.User, variantPermission))
            {
                context.Succeed(requirement);

                return;
            }
        }

        if (context.Resource is User user)
        {
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (requirement.Permission.Name == CommonPermissions.EditUsers.Name
                && user.UserId == currentUserId
                && await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.EditOwnUser))
            {
                context.Succeed(requirement);

                return;
            }

            if (await _authorizationService.AuthorizeAsync(context.User, requirement.Permission))
            {
                context.Succeed(requirement);

                return;
            }

            var roleNames = user.RoleNames ?? Enumerable.Empty<string>();

            if (!roleNames.Any())
            {
                // When the user is in no roles, we check to see if the current user can manage any roles.
                roleNames = (await _roleService.GetRoleNamesAsync()).Where(roleName => !RoleHelper.SystemRoleNames.Contains(roleName));
            }

            // Check every role to see if the current user has permission to at least one role.
            foreach (var roleName in roleNames)
            {
                var variantPermission = GetPermissionVariation(requirement.Permission, roleName);

                if (variantPermission != null && await _authorizationService.AuthorizeAsync(context.User, variantPermission))
                {
                    context.Succeed(requirement);

                    return;
                }
            }
        }
    }

    private static Permission GetPermissionVariation(Permission permission, string roleName)
    {
        if (permission.Name == CommonPermissions.ListUsers.Name)
        {
            return CommonPermissions.CreateListUsersInRolePermission(roleName);
        }

        if (permission.Name == CommonPermissions.EditUsers.Name)
        {
            return CommonPermissions.CreateEditUsersInRolePermission(roleName);
        }

        if (permission.Name == CommonPermissions.DeleteUsers.Name)
        {
            return CommonPermissions.CreateDeleteUsersInRolePermission(roleName);
        }

        if (permission.Name == CommonPermissions.AssignRoleToUsers.Name)
        {
            return CommonPermissions.CreateAssignRoleToUsersPermission(roleName);
        }

        if (permission.Name == CommonPermissions.ManageUsers.Name)
        {
            return CommonPermissions.CreatePermissionForManageUsersInRole(roleName);
        }

        return null;
    }
}
