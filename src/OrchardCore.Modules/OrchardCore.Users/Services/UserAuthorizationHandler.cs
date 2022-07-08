using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// This provides authorization when the permission request is ManageUsers and the current authenticated User
    /// has permission to edit any of the roles the user being managed is a member of.
    /// Callers should supply a <see cref="IUser"/> resource
    /// </summary>
    /// <remarks>
    /// This handler will not be triggered when only requesting ManagerUsers permission with supplying an <see href="IUser"/>.
    /// </remarks>
    public class UserAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<IUser> _userManager;
        private readonly IRoleService _roleService;

        public UserAuthorizationHandler(
            IServiceProvider serviceProvider,
            UserManager<IUser> userManager,
            IRoleService roleService)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
            _roleService = roleService;
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

            if (!String.Equals(requirement.Permission.Name, Permissions.ManageUsers.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var user = context.Resource as IUser;

            if (user == null)
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            var userRoleNames = await _userManager.GetRolesAsync(user);

            if (userRoleNames.Any())
            {
                // When the user has roles we check to see if any of these roles are not authorized before succeeding.
                if (await AuthorizeRolesAsync(authorizationService, context.User, userRoleNames))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                var roleNames = await _roleService.GetRoleNamesAsync();
                // When the user is in no roles, we check to see if the current user can manage any roles.
                if (await HasAuthorizationToManageAnyRoleAsync(authorizationService, context.User, roleNames))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private async Task<bool> AuthorizeRolesAsync(IAuthorizationService authorizationService, ClaimsPrincipal user, IEnumerable<string> roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (!await authorizationService.AuthorizeAsync(user, CommonPermissions.CreatePermissionForManageUsersInRole(roleName)))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> HasAuthorizationToManageAnyRoleAsync(IAuthorizationService authorizationService, ClaimsPrincipal user, IEnumerable<string> roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (await authorizationService.AuthorizeAsync(user, CommonPermissions.CreatePermissionForManageUsersInRole(roleName)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
