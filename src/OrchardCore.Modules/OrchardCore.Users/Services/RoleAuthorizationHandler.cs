using System;
using System.Collections.Generic;
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
            if (await TryAuthenticateAsync(context, requirement.Permission, role.RoleName))
            {
                context.Succeed(requirement);

                return;
            }
        }

        if (context.Resource is User user)
        {
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (String.Equals(requirement.Permission.Name, CommonPermissions.EditUsers.Name, StringComparison.OrdinalIgnoreCase)
                && user.UserId != null && user.UserId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)
                && await _authorizationService.AuthorizeAsync(context.User, Permissions.EditOwnUserInformation))
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
                // if the provided user does not have role mean it's a dummy user and we'll check to see if the current user has access to any permission variant
                roleNames = await AvailableRolesAsync();
            }

            foreach (var roleName in roleNames)
            {
                if (await TryAuthenticateAsync(context, requirement.Permission, roleName))
                {
                    context.Succeed(requirement);

                    return;
                }
            }

        }
    }

    private IEnumerable<string> _roleNames;

    private async Task<IEnumerable<string>> AvailableRolesAsync()
    {
        if (_roleNames == null)
        {
            _roleNames = (await _roleService.GetRoleNamesAsync())
                           .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase);
        }

        return _roleNames;
    }

    private async Task<bool> TryAuthenticateAsync(AuthorizationHandlerContext context, Permission permission, string roleName)
    {
        if (String.Equals(permission.Name, CommonPermissions.ListUsers.Name, StringComparison.OrdinalIgnoreCase)
            && await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CreateListUsersInRolePermission(roleName)))
        {
            return true;
        }

        if (String.Equals(permission.Name, CommonPermissions.EditUsers.Name, StringComparison.OrdinalIgnoreCase)
            && await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CreateEditUsersInRolePermission(roleName)))
        {
            return true;
        }

        if (String.Equals(permission.Name, CommonPermissions.DeleteUsers.Name, StringComparison.OrdinalIgnoreCase)
            && await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CreateDeleteUsersInRolePermission(roleName)))
        {
            return true;
        }

        if (String.Equals(permission.Name, CommonPermissions.AssignRole.Name, StringComparison.OrdinalIgnoreCase)
            && await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CreateAssignUserToRolePermission(roleName)))
        {
            return true;
        }

        return false;
    }
}
